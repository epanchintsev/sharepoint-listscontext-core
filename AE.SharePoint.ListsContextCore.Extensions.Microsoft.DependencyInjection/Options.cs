using System.Net;

namespace AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection
{
    public class Options
    {
        public string SharePointSiteUrl { get; set; }

        public ICredentials Credentials { get; set; }
    }
}
