using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AE.SharePoint.ListsContextCore.Infrastructure;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Represents a SharePoint list.
    /// </summary>
    /// <typeparam name="T">Type of the class that represents fields model of the SharePoint list.</typeparam>
    public sealed class SharePointList<T> : SharePointListBase<T> where T : new()
    {
        private static string sharePointTypeName;
        
        private readonly SharePointRestApiClient restApiClient;
        private readonly IConverter converter;
        private readonly FormDigestStorage formDigestStorage;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePoint list with the specified
        /// HttpClient and SharePoint list name.
        /// </summary>
        /// <param name="restApiClient">The instance of SharePointRestApiClient.</param>
        /// <param name="formDigestStorage"></param>
        /// <param name="listName">The name of the SharePoint list, displayed at the SharePoint site.</param>
        internal SharePointList(SharePointRestApiClient restApiClient, FormDigestStorage formDigestStorage, string listName): base(listName)
        {
            this.restApiClient = restApiClient;
            this.formDigestStorage = formDigestStorage;
            this.converter = new SharePointJsonConverter(PropertiesCreationInfo);
        }

        internal async Task<string> GetSharePointTypeNameAsync()
        {            
            if(string.IsNullOrEmpty(sharePointTypeName))
            {
                sharePointTypeName = await GetSharePointEntityTypeFullNameAsync();
            }

            return sharePointTypeName;
        }

        /// <summary>
        /// Returns all the items from the list.
        /// </summary>
        /// <returns>The Task object of strongly typed object list.</returns>
        public async Task<List<T>> GetAllItemsAsync()
        {
            var parameters = new ApiRequestParameters { Select = GetSelectParameter(), Top = 10000 };
            var json = await restApiClient.GetItemsAsync(listName, parameters);
            var result = converter.ConvertFromSPEntities<T>(json);

            return result;
        }


        /// <summary>
        /// Returns item with particular Id.
        /// </summary>
        /// <param name="id">Id of the target element.</param>
        /// <returns>The Task object of strongly typed object.</returns>
        public async Task<T> GetItemAsync(int id)
        {            
            var parameters = new ApiRequestParameters { Select = GetSelectParameter()};
            var json = await restApiClient.GetItemAsync(listName, id, parameters);
            var result = converter.ConvertFromSPEntity<T>(json);
            
            return result;
        }

        /// <summary>
        /// Returns a collection of items from the list based on the specified query.
        /// </summary>
        /// <param name="query">CAML query as string.</param>
        /// <returns></returns>
        public async Task<List<T>> GetItemsAsync(string query)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            var parameters = new ApiRequestParameters { Select = GetSelectParameter(), Top = 10000 };
            var json = await restApiClient.GetItemsAsync(listName, digest, query, parameters);
            var result = converter.ConvertFromSPEntities<T>(json);

            return result;
        }

        public async Task AddItemAsync(T item)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            string type = await GetSharePointTypeNameAsync();
            var json = converter.ConvertToSPEntity<T>(item, type);
            await restApiClient.AddItemAsync(listName, digest, json);
        }

        public async Task UpdateItemAsync(T item)
        {
            var id = ((IListItemBase)item).Id;
            var digest = await formDigestStorage.GetFormDigestAsync();
            string type = await GetSharePointTypeNameAsync();
            var json = converter.ConvertToSPEntity<T>(item, type);
            await restApiClient.UpdateItemAsync(listName, id, digest, json);
        }

        public async Task DeleteItemAsync(int id)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            await restApiClient.DeleteItemAsync(listName, digest, id);
        }

        private string GetSelectParameter()
        {
            var selectParameter = string.Join(',', PropertiesCreationInfo.Select(x => x.SharePointFieldName));

            return selectParameter;    
        }

        private string GetExpandParameter()
        {
            return string.Empty;
        }

        private async Task<string> GetSharePointEntityTypeFullNameAsync()
        {
            var parameters = new ApiRequestParameters { Select = "ListItemEntityTypeFullName" };
            var json = await restApiClient.GetListAsync(listName, parameters);
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);
            var sharePointEntityTypeFullName = jsonDocument.RootElement.GetProperty("d").GetProperty("ListItemEntityTypeFullName").ToString();

            return sharePointEntityTypeFullName;
        }

        
    }
}
