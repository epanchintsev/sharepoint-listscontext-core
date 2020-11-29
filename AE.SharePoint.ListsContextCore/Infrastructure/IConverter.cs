using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal interface IConverter
    {
        T ConvertFromSPEntity<T>(Object source) where T : new();

        List<T> ConvertFromSPEntities<T>(object source) where T : new();

        string ConvertToSPEntity<T>(Object source, string sharePointTypeName);
    }
}
