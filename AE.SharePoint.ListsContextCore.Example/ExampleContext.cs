using AE.SharePoint.ListsContextCore.Example.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Example
{
    class ExampleContext: SharePointListsContext
    {
        public ExampleContext(HttpClient client) :
            base(
                client,
                new ContextOptions
                {
                    DatesFromText = true,
                    DatesFromTextFormat = "dd.MM.YYYY hh:mm"
                }
            )
        {            
        }

        [SharePointListName("ArticlesList")]
        public SharePointList<ArticleListItem> Articles { get; set; }
    }
}
