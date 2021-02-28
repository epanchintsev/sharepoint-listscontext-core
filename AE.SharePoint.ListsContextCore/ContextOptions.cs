using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Specifies context options.
    /// </summary>
    public class ContextOptions
    {
        /// <summary>
        /// Set to true to getting value of DateTime type property as displaed on SharePoint form.
        /// </summary>
        public bool DatesFromText { get; set; }

        /// <summary>
        /// Specifies the format of conversion from string to DateTime when using dates from text.
        /// </summary>
        public string DatesFromTextFormat { get; set; }
    }
}
