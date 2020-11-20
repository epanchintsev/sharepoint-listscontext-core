using System;

using Microsoft.Extensions.DependencyInjection;

using AE.SharePoint.ListsContextCore.Infrastructure;

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

            serviceCollection.AddScoped<T>();

            serviceCollection
                .AddHttpClient<SharePointListsContext>(client => HttpClientHelper.ConfigureHttpClient(client, opt))
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.GetHttpClientHandler(opt.Credentials))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }
    }
}
