using System;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection;

namespace AE.SharePoint.ListsContextCore.Example
{
    class Program
    {
        private const string userName = "epanchintsev";
        private const string password = "";
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var serviceProvider = CreateServiceProvider();
            
            var context = serviceProvider.GetService<ExampleContext>();

            var items = context.List.GetAllItemsAsync().Result;

            var item = context.List.GetItemAsync(1).Result;

            var viewXml = new {
                ViewXml =   "<View>" +
                                "<Query>" +
                                    "<Where><Eq>" +
                                        "<FieldRef Name='Category' LookupId='True' />" +
                                        "<Value Type='Lookup'>1</Value>" +
                                    "</Eq></Where>" +
                                "</Query>" +
                            "</View>"
                        };

        var selectedItems = context.List.GetItemsAsync(viewXml.ToString()).Result;
        }

        private static ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSharePointListsContext<ExampleContext>(options =>
            {
                options.SharePointSiteUrl = "http://sharepointsite.url/sites/test-site";
                options.Credentials = new NetworkCredential(userName, password);
            }); 

            var serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
