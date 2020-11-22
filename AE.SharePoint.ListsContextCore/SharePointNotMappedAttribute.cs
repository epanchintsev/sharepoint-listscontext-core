using System;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Attribute used to mark the property of the SharePoint list model that does not have corresponting field in list, 
    /// and should not initialized by value from SharePoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointNotMappedAttribute: Attribute
    {
    }
}
