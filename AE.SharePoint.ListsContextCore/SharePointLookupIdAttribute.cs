using System;

namespace AE.SharePoint.ListsContextCore
{
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointLookupIdAttribute: SharePointFieldTypeAttribute
    {
        public SharePointLookupIdAttribute(): base(Infrastructure.SharePointFieldType.LookupId)
        {
        }
    }
}
