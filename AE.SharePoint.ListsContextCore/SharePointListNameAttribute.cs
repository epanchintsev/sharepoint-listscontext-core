using System;

namespace AE.SharePoint.ListsContextCore
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointListNameAttribute: Attribute
    {
        private string name;
        
        public SharePointListNameAttribute(string name)
        {
            this.name = name;
        }

        public string Name => name;
    }
}
