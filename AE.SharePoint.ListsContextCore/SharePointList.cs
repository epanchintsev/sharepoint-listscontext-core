using AE.SharePoint.ListsContextCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore
{
    public class SharePointList<T> where T: new()
    {
        private readonly HttpClient httpClient;
        private readonly string listName;
        private readonly IConverter converter;

        public SharePointList(HttpClient httpClient, string listName)
        {
            this.httpClient = httpClient;
            this.listName = listName;
            this.converter = new SharePointJsonConverter(); //TODO: подумать как сделать через внедрение зависимостей.
        }
        
        
        public async Task<List<T>> GetAllItemsAsync()
        {
            var path = $"/_api/web/lists/GetByTitle('{listName}')/items";

            //Для выбора полей GET https://{site_url}/_api/web/lists('{list_guid}')/items?$select=Title,Products/Name&$expand=Products/Name

            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.Convert<List<T>>(json);

            return result;
        }

        public async Task<T> GetItemAsync(int id)
        {
            var path = $"/_api/web/lists/GetByTitle('{listName}')/items({id})";
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.Convert<T>(json);
            
            return result;
        }

        public async Task<List<T>> GetItemsAsync(string query)
        {
            var path = $"/_api/web/lists/GetByTitle('{listName}')/GetItems(query=@v1)?@v1={query}";
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.Convert<List<T>>(json);

            return result;
        }
    }
}
