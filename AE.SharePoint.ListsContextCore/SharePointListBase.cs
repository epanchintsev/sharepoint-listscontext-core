﻿using AE.SharePoint.ListsContextCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    public abstract class SharePointListBase<T>
    {
        private static List<ListItemPropertyCreationInfo> propertiesCreationInfo;

        protected readonly string listName;

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