using System.Net;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Represents options to configure AE.SharePoint.ListsContextCore.SharePointListsContext.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// SharePoint site address. Include protocol and trailing slash at the end of url.
        /// </summary>
        public string SharePointSiteUrl { get; set; }

        /// <summary>
        /// Credentials to access the SharePoint site.
        /// If not set, default credentials will be used.
        /// </summary>
        public ICredentials Credentials { get; set; }

        public ContextOptions ContextOptions { get; set; }
    }
}
