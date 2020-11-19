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
    public class SharePointList<T> where T : new()
    {
        private static List<ListItemPropertyCreationInfo> propertiesCreationInfo;

        private readonly HttpClient httpClient;
        private readonly string listName;
        private readonly IConverter converter;

        public SharePointList(HttpClient httpClient, string listName)
        {
            this.httpClient = httpClient;
            this.listName = listName;
            this.converter = new SharePointJsonConverter(propertiesCreationInfo); //TODO: подумать как сделать через внедрение зависимостей.
        }

        private List<ListItemPropertyCreationInfo> PropertiesCreationInfo
        {
            get
            {
                if (propertiesCreationInfo == null)
                {
                    propertiesCreationInfo = GetPropertiesCreationInfo();
                }

                return propertiesCreationInfo;
            }
        }


        public async Task<List<T>> GetAllItemsAsync()
        {
            var path = $"/_api/web/lists/GetByTitle('{listName}')/items";

            //Для выбора полей GET https://{site_url}/_api/web/lists('{list_guid}')/items?$select=Title,Products/Name&$expand=Products/Name

            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = converter.ConvertItems<T>(json);

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
            var result = converter.ConvertItems<T>(json);

            return result;
        }

        private List<ListItemPropertyCreationInfo> GetPropertiesCreationInfo()
        {
            Type selfType = typeof(T);

            var creationInfo = GetAllowedProperties(selfType)
                .Select(property =>
                    new ListItemPropertyCreationInfo
                    {
                        PropertyToSet = property,
                        SharePointFieldName = GetSharePointFieldName(property)
                    }
                )
                .ToList();

            return creationInfo;
        }

        private static IEnumerable<PropertyInfo> GetAllowedProperties(Type selfType)
        {
            
            // Берутся только свойства у которых есть set метод даже если он приватный.
            // Не берутся свойства, помеченные специальным атрибутом.
            IEnumerable<PropertyInfo> properties = selfType
                .GetProperties()
                .Where(p => p.CanWrite && p.GetCustomAttributes(typeof(SharePointNotMappedAttribute)).Count() == 0);

            //TODO: Ограничить передаваемые свойства можно еще с помощью специальных методов Include и Exclude

            return properties;
        }

        public static string GetSharePointFieldName(PropertyInfo property)
        {
            var fieldNameAttribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is SharePointFieldNameAttribute);
            string fieldName = fieldNameAttribute != null ? ((SharePointFieldNameAttribute)fieldNameAttribute).Name : property.Name;
            return fieldName;
        }
    }
}
