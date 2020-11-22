using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AE.SharePoint.ListsContextCore.Infrastructure;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Abstract class incapsulate base logic for SharePointList.
    /// </summary>
    /// <typeparam name="T">Type of the class that represents fields model of the SharePoint list.</typeparam>
    public abstract class SharePointListBase<T>
    {
        private static List<ListItemPropertyCreationInfo> propertiesCreationInfo;

        /// <summary>
        /// Displayed name of the SharePointList.
        /// </summary>
        protected readonly string listName;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePoint list with the specified
        /// SharePoint list name.
        /// </summary>
        /// <param name="listName">Displayed name of the SharePointList.</param>
        public SharePointListBase(string listName)
        {
            this.listName = listName;
        }

        internal List<ListItemPropertyCreationInfo> PropertiesCreationInfo
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

        private static string GetSharePointFieldName(PropertyInfo property)
        {
            var fieldNameAttribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is SharePointFieldNameAttribute);
            string fieldName = fieldNameAttribute != null ? ((SharePointFieldNameAttribute)fieldNameAttribute).Name : property.Name;
            return fieldName;
        }
    }
}
