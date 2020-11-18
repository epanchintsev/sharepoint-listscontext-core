using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class SharePointJsonConverter : IConverter
    {
        public T Convert<T>(object source) where T: new()
        {
            return new T();
        }
    }
}
