﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal interface IConverter
    {
        T ConvertFromSPEntity<T>(Object source, IEnumerable<ListItemPropertyCreationInfo> properties) where T : new();

        List<T> ConvertFromSPEntities<T>(object source, IEnumerable<ListItemPropertyCreationInfo> properties) where T : new();

        string ConvertToSPEntity<T>(Object source, string sharePointTypeName, IEnumerable<ListItemPropertyCreationInfo> properties);
    }
}
