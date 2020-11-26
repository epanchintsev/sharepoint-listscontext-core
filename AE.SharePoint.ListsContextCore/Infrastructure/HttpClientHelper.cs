using System;
using System.Net;
using System.Net.Http;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    /// <summary>
    /// Helper class to configure System.Net.Http.HttpClient for using to access SharePoint REST API.
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// Configure System.Net.Http.HttpClient. Set base address and necessary headers.
        /// </summary>
        /// <param name="client">Instance of System.Net.Http.HttpClient that used in AE.SharePoint.ListsContextCore.SharePointListsContext.</param>
        /// <param name="options"></param>
        public static void ConfigureHttpClient(HttpClient client, Options options)
        {
            client.BaseAddress = new Uri(options.SharePointSiteUrl);

            client.DefaultRequestHeaders.Clear();
            if (options.Credentials == null || options.Credentials is NetworkCredential)
            {
                client.DefaultRequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            }
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
        }

        /// <summary>
        /// Returns configured System.Net.Http.HttpClientHandler whith set of authentication information.
        /// </summary>
        /// <param name="credentials">Credentials to access the SharePoint site. If not set, default credentials will be used.
        /// </param>
        /// <returns></returns>
        public static HttpClientHandler GetHttpClientHandler(ICredentials credentials)
        {
            HttpClientHandler handler;

            if (credentials == null)
            {
                handler = new HttpClientHandler()
                {
                    UseDefaultCredentials = true
                };
            }
            else
            {
                handler = new HttpClientHandler()
                {
                    Credentials = credentials
                };
            }

            return handler;
        }
    }
}
