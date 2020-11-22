using System;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Attribute is used to map a context property to a specific SharePoint list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointListNameAttribute: Attribute
    {
        private string name;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointListNameAttribute list with the specified name.
        /// </summary>
        /// <param name="name">Displayed name of the SharePointList.</param>
        public SharePointListNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Displayed name of the SharePointList.
        /// </summary>
        public string Name => name;
    }
}
