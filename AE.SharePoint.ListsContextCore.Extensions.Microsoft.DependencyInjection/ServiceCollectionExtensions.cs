using System;
using System.Net.Http;

using Microsoft.Extensions.DependencyInjection;

namespace AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSharePointListsContext<T>(this IServiceCollection serviceCollection, Action<Options> options) where T : SharePointListsContext
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Not null options parameter required.");
            }

            var opt = new Options();
            options.Invoke(opt);

            serviceCollection
                .AddHttpClient<SharePointListsContext>(client =>
                {
                    
                    client.BaseAddress = new Uri(opt.SharePointSiteUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f"); //TODO: возможно стоит сделать проверку и добавлять только если NetworkCredentials а для SharePointCredentials не добавлять.
                    client.DefaultRequestHeaders.Add("ContentType", "application/json;odata=verbose");
                    client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        UseDefaultCredentials = true, //TODO: проверить. возможно достаточно чего то одного.
                        Credentials = opt.Credentials
                    };
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            serviceCollection.AddScoped<T>();
        }
    }
}
