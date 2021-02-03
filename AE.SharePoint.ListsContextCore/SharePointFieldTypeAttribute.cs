using AE.SharePoint.ListsContextCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    public abstract class SharePointFieldTypeAttribute: Attribute
    {
        private SharePointFieldType type;

        internal SharePointFieldTypeAttribute(SharePointFieldType type)
        {
            this.type = type;
        }

        internal SharePointFieldType Type => type;
    }
}
