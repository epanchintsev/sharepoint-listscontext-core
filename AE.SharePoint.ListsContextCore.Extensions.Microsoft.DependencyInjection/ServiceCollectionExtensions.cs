using System;

using Microsoft.Extensions.DependencyInjection;

using AE.SharePoint.ListsContextCore.Infrastructure;

namespace AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to an Microsoft.Extensions.DependencyInjection.IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configure AE.SharePoint.ListsContextCore.SharePointListsContext.
        /// </summary>
        /// <typeparam name="T">The type of context class inherited from AE.SharePoint.ListsContextCore.SharePointListsContext</typeparam>
        /// <param name="serviceCollection">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the service to.</param>
        /// <param name="options">A delegate that is used to configure an AE.SharePoint.ListsContextCore.SharePointListsContext.</param>
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
                .AddHttpClient<T>(client => HttpClientHelper.ConfigureHttpClient(client, opt))
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.GetHttpClientHandler(opt.Credentials))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }
    }
}
