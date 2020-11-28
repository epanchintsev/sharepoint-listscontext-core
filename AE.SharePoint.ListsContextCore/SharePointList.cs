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

        /// <summary>
        /// Returns all the items from the list.
        /// </summary>
        /// <returns>The Task object of strongly typed object list.</returns>
        public async Task<List<T>> GetAllItemsAsync()
        {
            //Для выбора полей GET https://{site_url}/_api/web/lists('{list_guid}')/items?$select=Title,Products/Name&$expand=Products/Name

            var path = $"_api/web/lists/GetByTitle('{listName}')/items?$select={GetSelectParameter()}&$top=10000";
            var json = await restApiClient.GetAsync(path);
            var result = converter.ConvertItems<T>(json);

            return result;
        }


        /// <summary>
        /// Returns item with particular Id.
        /// </summary>
        /// <param name="id">Id of the target element.</param>
        /// <returns>The Task object of strongly typed object.</returns>
        public async Task<T> GetItemAsync(int id)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items({id})?$select={GetSelectParameter()}";
            var json = await restApiClient.GetAsync(path);
            var result = converter.Convert<T>(json);
            
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
            var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems?$select={GetSelectParameter()}";
            var data = new { query = new { __metadata = new { type = "SP.CamlQuery" }, ViewXml = query }};
            var json = await restApiClient.PostAsync(path, digest, data);
            var result = converter.ConvertItems<T>(json);

            return result;
        }

        public async Task DeleteItemAsync(int id)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            var path = $"_api/web/lists/GetByTitle('{listName}')/items({id})";            
            await restApiClient.DeleteAsync(path, digest);
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

        
    }
}
