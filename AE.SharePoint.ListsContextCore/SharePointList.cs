﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private readonly HttpClient httpClient;
        private readonly IConverter converter;
        private readonly FormDigestStorage formDigestStorage;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePoint list with the specified
        /// HttpClient and SharePoint list name.
        /// </summary>
        /// <param name="httpClient">The instance of HttpClient that used to access to SharePoint REST API.</param>
        /// <param name="formDigestStorage"></param>
        /// <param name="listName">The name of the SharePoint list, displayed at the SharePoint site.</param>
        internal SharePointList(HttpClient httpClient, FormDigestStorage formDigestStorage, string listName): base(listName)
        {
            this.httpClient = httpClient;
            this.formDigestStorage = formDigestStorage;
            this.converter = new SharePointJsonConverter(PropertiesCreationInfo);
        }

        /// <summary>
        /// Returns all the elements of the list.
        /// </summary>
        /// <returns>The Task object of strongly typed object list.</returns>
        public async Task<List<T>> GetAllItemsAsync()
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items?$select={GetSelectParameter()}&$top=10000";

            //Для выбора полей GET https://{site_url}/_api/web/lists('{list_guid}')/items?$select=Title,Products/Name&$expand=Products/Name

            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.ConvertItems<T>(json);

            return result;
        }


        /// <summary>
        /// Returns element with particular Id.
        /// </summary>
        /// <param name="id">Id of the target element.</param>
        /// <returns>The Task object of strongly typed object.</returns>
        public async Task<T> GetItemAsync(int id)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items({id})?$select={GetSelectParameter()}";
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.Convert<T>(json);
            
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<List<T>> GetItemsAsync(string query)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems(query=@v1)?@v1={{'ViewXml':'{query}'}}&$top=10000";
            var digest = await formDigestStorage.GetFormDigestAsync();
            
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
            {
                requestMessage.Headers.Add("X-RequestDigest", digest);                
                var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var result = converter.ConvertItems<T>(json);

                return result;
            }            
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


//public async Task<List<T>> GetItemsAsync(string query)
//{
//    //var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems(query=@v1)?@v1={query}";

//    var digest = await GetDigestAsync();

//    //query = "<View><Query></Query></View>";

//    var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems";
//    var dataObj = new { query = new { __metadata = new { type = "SP.CamlQuery" }, ViewXml = query } };
//    var dataJson = System.Text.Json.JsonSerializer.Serialize(dataObj);
//    var data = new StringContent(dataJson, Encoding.UTF8, "application/json");

//    //var response = await httpClient.PostAsync(path, data);


//    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
//    {
//        requestMessage.Headers.Add("X-RequestDigest", digest.FormDigestValue);
//        requestMessage.Content = data;
//        var response = await httpClient.SendAsync(requestMessage);

//        response.EnsureSuccessStatusCode();
//        var json = await response.Content.ReadAsStringAsync();
//        var result = converter.ConvertItems<T>(json);

//        return result;
//    }
//}
