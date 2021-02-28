using System;

namespace AE.SharePoint.ListsContextCore
{

    /// <summary>
    /// Attribute used to mark the property of the SharePoint list model that mapped to Lookup field and returns value of foreign item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointLookupValueAttribute: SharePointFieldTypeAttribute
    {
        private string pulledFieldName;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointLookupIdAttribute class.
        /// </summary>
        /// <param name="pulledFieldName">Name of the SharePoint field in the foreign list.</param>
        public SharePointLookupValueAttribute(string pulledFieldName) :base(Infrastructure.SharePointFieldType.LookupValue)
        {
            this.pulledFieldName = pulledFieldName;
        }

        internal string PulledFieldName => pulledFieldName;
    }
}
