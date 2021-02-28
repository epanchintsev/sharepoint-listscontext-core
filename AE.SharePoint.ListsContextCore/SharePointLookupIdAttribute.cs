using System;

namespace AE.SharePoint.ListsContextCore
{

    /// <summary>
    /// Attribute used to mark the property of the SharePoint list model that mapped to Lookup field and returns id of foreign item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointLookupIdAttribute: SharePointFieldTypeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointLookupIdAttribute class.
        /// </summary>
        public SharePointLookupIdAttribute(): base(Infrastructure.SharePointFieldType.LookupId)
        {
        }
    }
}
