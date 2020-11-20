using AE.SharePoint.ListsContextCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore
{
    public class SharePointList<T> : SharePointListBase<T> where T : new()
    {
        private readonly HttpClient httpClient;
        private readonly IConverter converter;

        public SharePointList(HttpClient httpClient, string listName): base(listName)
        {
            this.httpClient = httpClient;
            this.converter = new SharePointJsonConverter(PropertiesCreationInfo);
        }

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

        public async Task<T> GetItemAsync(int id)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items({id})?$select={GetSelectParameter()}";
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.Convert<T>(json);
            
            return result;
        }

        public async Task<List<T>> GetItemsAsync(string query)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems(query=@v1)?@v1={query}&$top=10000";
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.ConvertItems<T>(json);

            return result;
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
