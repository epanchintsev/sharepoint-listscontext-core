using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal interface IConverter
    {
        T Convert<T>(Object source) where T : new();
    }
}
