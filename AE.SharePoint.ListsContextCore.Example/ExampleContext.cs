using AE.SharePoint.ListsContextCore.Example.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Example
{
    class ExampleContext: SharePointListsContext
    {
        public ExampleContext(HttpClient client): base(client)
        {            
        }

        [SharePointListName("ExampleList")]
        public SharePointList<ExampleList> List { get; set; }
    }
}
