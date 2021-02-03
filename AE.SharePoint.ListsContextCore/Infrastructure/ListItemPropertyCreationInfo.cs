using System.Reflection;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class ListItemPropertyCreationInfo
    {
        public PropertyInfo PropertyToSet { get; set; }

        public string SharePointFieldName { get; set; }

        public SharePointFieldType SharePointFieldType { get; set; }

        public object AdditionalData { get; set; }
    }
}
