using System;

namespace AE.SharePoint.ListsContextCore
{
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointLookupValueAttribute: SharePointFieldTypeAttribute
    {
        private string pulledFieldName;

        public SharePointLookupValueAttribute(string pulledFieldName) :base(Infrastructure.SharePointFieldType.LookupValue)
        {
            this.pulledFieldName = pulledFieldName;
        }

        public string PulledFieldName => pulledFieldName;
    }
}
