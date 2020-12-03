using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Base class for List item model.
    /// </summary>
    public class ListItemBase: IListItemBase
    {
        /// <summary>
        /// List item unique identifier.
        /// </summary>
        public int Id { get; set; }
    }
}
