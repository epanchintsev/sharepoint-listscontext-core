using System.Reflection;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class ListItemPropertyCreationInfo
    {
        public PropertyInfo PropertyToSet { get; set; }

        public string SharePointFieldName { get; set; }
    }
}
