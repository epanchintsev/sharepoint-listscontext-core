using System;

namespace AE.SharePoint.ListsContextCore
{
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
