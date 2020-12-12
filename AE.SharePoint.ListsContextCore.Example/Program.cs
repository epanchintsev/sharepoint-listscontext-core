using System;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection;
using System.Collections.Generic;
using AE.SharePoint.ListsContextCore.Example.Models;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore.Example
{
    class Program
    {
        private const string userName = "epanchintsev";
        private const string password = "";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start!");
            
            var serviceProvider = CreateServiceProvider();
            
            var context = serviceProvider.GetService<ExampleContext>();

            List<ArticleListItem> items = await context.Articles.GetAllItemsAsync();

            ArticleListItem item = context.Articles.GetItemAsync(1).Result;

            string ViewXml = "<View>" +
                                "<Query>" +
                                    "<Where><Eq>" +
                                        "<FieldRef Name='Title' />" +
                                        "<Value Type='Text'>Happy New Year</Value>" +
                                    "</Eq></Where>" +
                                "</Query>" +
                            "</View>";

            List<ArticleListItem> selectedItems = await context.Articles.Take(1).GetItemsAsync(ViewXml);

            var newItem = new ArticleListItem
            {
                Title = "Happy New 2019 Year"
            };

            ArticleListItem createdItem = await context.Articles.AddItemAsync(newItem);

            List<ArticleListItem> itemsInShortForm = await context.Articles
                .ExcludeFields(x => new { x.Description })
                .Take(5)
                .GetAllItemsAsync();

            List<ArticleListItem> itemIds = await context.Articles
                .IncludeFields(x => new { x.Id })                
                .GetAllItemsAsync();
        }

        private static ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSharePointListsContext<ExampleContext>(options =>
            {
                options.SharePointSiteUrl = "http://sharepointsite.url/sites/test-site/"; //Слэш на конце обязателен.
                // options.Credentials = new NetworkCredential(userName, password);
            }); 

            var serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
