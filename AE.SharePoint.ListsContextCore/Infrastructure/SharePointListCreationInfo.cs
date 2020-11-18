using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class SharePointListCreationInfo
    {
        public PropertyInfo PropertyToSet { get; set; }

        public string ListName { get; set; }

        public ConstructorInfo PropertyInstanceConstructor { get; set; }
    }
}
