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

        [SharePointFieldName("Created")]
        public DateTime PublicationDate { get; set; }

        [SharePointNotMapped]
        public string Year { get; set; }

        [SharePointLookupId]
        public int AuthorId { get; set; }

        [SharePointLookupValue("Name")]
        public int AuthorName { get; set; }
    }
}
