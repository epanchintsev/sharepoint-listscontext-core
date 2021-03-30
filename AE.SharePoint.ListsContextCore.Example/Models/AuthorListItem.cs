using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Example.Models
{
    class AuthorListItem: ListItemBase
    {
        public string Name { get; set; }

        SharePointUrlField Photo { get; set; }
    }
}
