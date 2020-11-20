using AE.SharePoint.ListsContextCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace AE.SharePoint.ListsContextCore
{
    public class SharePointListsContext
    {
        private static List<SharePointListCreationInfo> properties;
        
        private readonly HttpClient httpClient;
        
        public SharePointListsContext(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            if (properties == null)
            {
                properties = GetPropertiesCreationInfo();
            }

            InitSharePointListProperties();
        }

        private void InitSharePointListProperties()
        {
            foreach (var property in properties)
            {
                var propertyInstance = property.PropertyInstanceConstructor.Invoke(new object[] { httpClient, property.ListName });
                property.PropertyToSet.SetValue(this, propertyInstance);
            }
        }

        private List<SharePointListCreationInfo> GetPropertiesCreationInfo()
        {
            IEnumerable<PropertyInfo> spListProperties = this
                .GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(SharePointList<>));

            var constructorArguments = new Type[2]
                {
                    typeof(HttpClient),
                    typeof(string)
                };

            var properties = spListProperties
                .Select(property =>
                {
                    Type listItemType = property.PropertyType.GetGenericArguments()[0];

                    return new SharePointListCreationInfo
                    {
                        PropertyToSet = property,
                        ListName = GetListName(property),
                        PropertyInstanceConstructor = typeof(SharePointList<>).MakeGenericType(listItemType).GetConstructor(constructorArguments)
                    };
                })
                .ToList();

            return properties;
        }

        private static string GetListName(PropertyInfo property)
        {
            var listNameAttribute = property
                    .GetCustomAttributes(true)
                    .FirstOrDefault(a => a is SharePointListNameAttribute);

            string listName = listNameAttribute != null ?
                ((SharePointListNameAttribute)listNameAttribute).Name :
                property.Name;

            return listName;
        }
    }
}
