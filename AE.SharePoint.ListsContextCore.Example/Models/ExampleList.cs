using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Example.Models
{
    class ArticleListItem: IListItemBase
    {        
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
