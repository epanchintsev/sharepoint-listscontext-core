using System;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection;
using System.Collections.Generic;
using AE.SharePoint.ListsContextCore.Example.Models;

namespace AE.SharePoint.ListsContextCore.Example
{
    class Program
    {
        private const string userName = "epanchintsev";
        private const string password = "";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Start!");
            
            var serviceProvider = CreateServiceProvider();
            
            var context = serviceProvider.GetService<ExampleContext>();
            List<ArticleListItem> items = context.Articles.GetAllItemsAsync().Result;

            ArticleListItem item = context.Articles.GetItemAsync(1).Result;

            string ViewXml = "<View>" +
                                "<Query>" +
                                    "<Where><Eq>" +
                                        "<FieldRef Name='Title' />" +
                                        "<Value Type='Text'>Happy New Year</Value>" +
                                    "</Eq></Where>" +
                                "</Query>" +
                            "</View>";

            List<ArticleListItem> selectedItems = context.Articles.GetItemsAsync(ViewXml).Result;
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
