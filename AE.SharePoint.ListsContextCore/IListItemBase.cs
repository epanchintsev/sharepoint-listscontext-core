using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Base interface for List item model.
    /// </summary>
    public interface IListItemBase
    {
        /// <summary>
        /// List item unique identifier.
        /// </summary>
        int Id { get; set; }
    }
}
