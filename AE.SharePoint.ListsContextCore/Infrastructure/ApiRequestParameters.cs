using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class ApiRequestParameters
    {
        public string Select { get; set; }

        public string Expand { get; set; }

        public int Top { get; set; } = 100;
    }
}
