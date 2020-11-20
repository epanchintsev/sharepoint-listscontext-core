using System;
using System.Net;
using System.Net.Http;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    public static class HttpClientHelper
    {
        public static void ConfigureHttpClient(HttpClient client, Options options)
        {
            client.BaseAddress = new Uri(options.SharePointSiteUrl);

            client.DefaultRequestHeaders.Clear();
            if (options.Credentials == null || options.Credentials is NetworkCredential)
            {
                client.DefaultRequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            }
            client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
        }
        
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
