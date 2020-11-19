using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal interface IConverter
    {
        List<T> ConvertItems<T>(object source) where T : new();


        T Convert<T>(Object source) where T : new();
    }
}
