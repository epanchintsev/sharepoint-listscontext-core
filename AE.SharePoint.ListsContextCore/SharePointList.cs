using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AE.SharePoint.ListsContextCore.Infrastructure;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Represents a SharePoint list.
    /// </summary>
    /// <typeparam name="T">Type of the class that represents fields model of the SharePoint list.</typeparam>
    public sealed class SharePointList<T> : SharePointListBase<T> where T : class, new()
    {
        private static string sharePointTypeName;
        
        private readonly SharePointRestApiClient restApiClient;
        private readonly IConverter converter;
        private readonly FormDigestStorage formDigestStorage;

        private int top;
        private string[] includedFields;
        private string[] excludedFields;        

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePoint list with the specified
        /// HttpClient and SharePoint list name.
        /// </summary>
        /// <param name="restApiClient">The instance of SharePointRestApiClient.</param>
        /// <param name="formDigestStorage"></param>
        /// <param name="converter"></param>
        /// <param name="listName">The name of the SharePoint list, displayed at the SharePoint site.</param>
        internal SharePointList(SharePointRestApiClient restApiClient, FormDigestStorage formDigestStorage, IConverter converter, string listName): base(listName)
        {
            this.restApiClient = restApiClient;
            this.formDigestStorage = formDigestStorage;
            this.converter = converter;

            ResetParams();
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
            var usedProperties = GetUsedProperties();
            var parameters = new ApiRequestParameters { Select = GetSelectParameter(usedProperties), Top = top };
            var json = await restApiClient.GetItemsAsync(listName, parameters);
            var result = converter.ConvertFromSPEntities<T>(json, usedProperties);
            ResetParams();

            return result;
        }


        /// <summary>
        /// Returns item with specified Id.
        /// </summary>
        /// <param name="id">Id of the target item.</param>
        /// <returns>The Task object of strongly typed object.</returns>
        public async Task<T> GetItemAsync(int id)
        {
            var usedProperties = GetUsedProperties();
            var parameters = new ApiRequestParameters { Select = GetSelectParameter(usedProperties) };
            var json = await restApiClient.GetItemAsync(listName, id, parameters);
            var result = converter.ConvertFromSPEntity<T>(json, usedProperties);
            ResetParams();

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
            var usedProperties = GetUsedProperties();
            var parameters = new ApiRequestParameters { Select = GetSelectParameter(usedProperties), Top = top };
            var json = await restApiClient.GetItemsAsync(listName, digest, query, parameters);
            var result = converter.ConvertFromSPEntities<T>(json, usedProperties);
            ResetParams();

            return result;
        }

        /// <summary>
        /// Adds item to list.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <returns>Created item.</returns>
        public async Task<T> AddItemAsync(T item)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            string type = await GetSharePointTypeNameAsync();            
            var json = converter.ConvertToSPEntity<T>(item, type, PropertiesCreationInfo);
            var resultJson = await restApiClient.AddItemAsync(listName, digest, json); //TODO: возможно стоит сделать ограничение возввращаемых полей.
            
            if(string.IsNullOrEmpty(resultJson))
            {
                return null;
            }

            var result = converter.ConvertFromSPEntity<T>(resultJson, PropertiesCreationInfo); //Пока возвращаются все свойства.
            ResetParams();

            return result;
        }

        /// <summary>
        /// Updates item in list.
        /// </summary>
        /// <param name="item">Item with new values of properties. 
        /// Must be inherited from IListItemBase, and have Id property.
        /// Id property specifies which item should be updated.</param>
        /// <returns></returns>
        public async Task UpdateItemAsync(T item)
        {
            var id = ((IListItemBase)item).Id;
            var digest = await formDigestStorage.GetFormDigestAsync();
            string type = await GetSharePointTypeNameAsync();
            var usedProperties = GetUsedProperties();
            var json = converter.ConvertToSPEntity<T>(item, type, usedProperties);
            await restApiClient.UpdateItemAsync(listName, id, digest, json);
            ResetParams();
        }

        /// <summary>
        /// Deletes item with specified Id.
        /// </summary>
        /// <param name="id">Id of the target item.</param>
        /// <returns></returns>
        public async Task DeleteItemAsync(int id)
        {
            var digest = await formDigestStorage.GetFormDigestAsync();
            await restApiClient.DeleteItemAsync(listName, digest, id);
            ResetParams();
        }


        /// <summary>
        /// Specifies quantity of returned items.
        /// </summary>
        /// <param name="count">Quantity of returned items.</param>
        /// <returns></returns>
        public SharePointList<T> Take(int count)
        {
            if(count <= 0)
            {
                throw new ArgumentException($"The value of {nameof(count)} must be greater than zero.");
            }
            
            top = count;
            return this;
        }

        /// <summary>
        /// Determines for which fields the data should be received from SharePoint.
        /// Properties for which fields are not included in the set will have default values.
        /// </summary>
        /// <param name="fields">Set of properties the data for which to get from SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> IncludeFields(Expression<Func<T,object>> fields)
        {
            includedFields = GetNamesFromExpression(fields);
            return this;
        }

        /// <summary>
        /// Determines for which fields the data should not be received from SharePoint.
        /// Properties for which fields are excluded will have default values.
        /// </summary>
        /// <param name="fields">Set of properties the data for which to should not get from SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> ExcludeFields(Expression<Func<T, object>> fields)
        {
            excludedFields = GetNamesFromExpression(fields);
            return this;
        }

        private List<ListItemPropertyCreationInfo> GetUsedProperties()
        {
            if(includedFields.Length == 0 && excludedFields.Length == 0)
            {
                return PropertiesCreationInfo;
            }
            
            var usedProperties = PropertiesCreationInfo
                .Where(x =>
                    (includedFields.Length == 0 || includedFields.Contains(x.PropertyToSet.Name)) &&
                    (excludedFields.Length == 0 || !excludedFields.Contains(x.PropertyToSet.Name))
                )
                .ToList();

            return usedProperties;
        }

        private string GetSelectParameter(IEnumerable<ListItemPropertyCreationInfo> properties)
        {
            var selectParameter = string.Join(",", properties.Select(x => x.SharePointFieldName));
            return selectParameter;    
        }

        private string GetExpandParameter()
        {
            return string.Empty;
        }

        private void ResetParams()
        {
            top = 10000;
            includedFields = new string[0];
            excludedFields = new string[0];
        }

        private async Task<string> GetSharePointEntityTypeFullNameAsync()
        {
            var parameters = new ApiRequestParameters { Select = "ListItemEntityTypeFullName" };
            var json = await restApiClient.GetListAsync(listName, parameters);
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);
            var sharePointEntityTypeFullName = jsonDocument.RootElement.GetProperty("d").GetProperty("ListItemEntityTypeFullName").ToString();

            return sharePointEntityTypeFullName;
        }

        private string[] GetNamesFromExpression(Expression<Func<T, object>> expr)
        {
            var x = ((NewExpression)expr.Body).Members;
            string[] names = x.Select(m => m.Name).ToArray();
            return names;
        }
    }
}
