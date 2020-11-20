using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointFieldNameAttribute: Attribute
    {
        private string name;

        public SharePointFieldNameAttribute(string name)
        {
            this.name = name;
        }

        public string Name => name;
    }
}
