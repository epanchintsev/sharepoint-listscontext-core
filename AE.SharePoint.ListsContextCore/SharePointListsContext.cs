using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using AE.SharePoint.ListsContextCore.Infrastructure;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Base class for creating context of the SharePoint lists.
    /// </summary>
    public class SharePointListsContext
    {
        private static List<SharePointListCreationInfo> properties;
        
        private readonly FormDigestStorage formDigestStorage;
        private readonly SharePointRestApiClient restApiClient;        

        public bool DatesFromText { get; private set; }

        public string DatesFromTextFormat { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointListsContext list with the specified
        /// HttpClient and default options.
        /// </summary>
        /// <param name="httpClient">The instance of HttpClient that used to access to SharePoint REST API.</param>
        public SharePointListsContext(HttpClient httpClient)
        {
            restApiClient = new SharePointRestApiClient(httpClient);
            formDigestStorage = new FormDigestStorage(restApiClient);

            if (properties == null)
            {
                properties = GetPropertiesCreationInfo();
            }

            InitSharePointListProperties();
        }

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointListsContext list with the specified
        /// HttpClient.
        /// </summary>
        /// <param name="httpClient">The instance of HttpClient that used to access to SharePoint REST API.</param>
        /// <param name="options"></param>
        public SharePointListsContext(HttpClient httpClient, ContextOptions options) : this(httpClient)
        {
            DatesFromText = options.DatesFromText;
            DatesFromTextFormat = options.DatesFromTextFormat;            
        }

        private void InitSharePointListProperties()
        {
            foreach (var property in properties)
            {
                var converter = new SharePointJsonConverter(DatesFromText, DatesFromTextFormat);
                var propertyInstance = property.PropertyInstanceConstructor.Invoke(new object[] { restApiClient, formDigestStorage, converter, this, property.ListName });
                property.PropertyToSet.SetValue(this, propertyInstance);
            }
        }

        private List<SharePointListCreationInfo> GetPropertiesCreationInfo()
        {
            IEnumerable<PropertyInfo> spListProperties = this
                .GetType()
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(SharePointList<>));

            var properties = spListProperties
                .Select(property =>
                {
                    Type listItemType = property.PropertyType.GetGenericArguments()[0];

                    return new SharePointListCreationInfo
                    {
                        PropertyToSet = property,
                        ListName = GetListName(property),
                        PropertyInstanceConstructor = typeof(SharePointList<>)
                            .MakeGenericType(listItemType)
                            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
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
