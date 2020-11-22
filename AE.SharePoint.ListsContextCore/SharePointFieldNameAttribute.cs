using System;

namespace AE.SharePoint.ListsContextCore
{

    /// <summary>
    /// Attribute is used to map a model property to a specific SharePoint list field, if field name and property name are different.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointFieldNameAttribute: Attribute
    {
        private string name;

        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.SharePointFieldNameAttribute list with the specified name.
        /// </summary>
        /// <param name="name">Original name of a SharePoint field.</param>
        public SharePointFieldNameAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Original name of a SharePoint field.
        /// </summary>
        public string Name => name;
    }
}
