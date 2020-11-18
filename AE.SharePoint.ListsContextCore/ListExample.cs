public class SharePointList<T> : ISharePointList<T> where T : new()
    {
        private const int maxItemsInDeleteQuery = 40; //было 40 - работало, но хочется же по быстрее. 60 - иногда не работает.
        private const string attachmentsFieldName = "AttachmentFiles";

        private ClientContext context;
        private string listName;

        private List<string> allowedPropertyNames;
        private List<string> disAllowedPropertyNames;

        /// <summary>
        /// Создает экземпляр класса.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="listName"></param>
        public SharePointList(ClientContext context, string listName)
        {
            this.context = context;
            this.listName = listName;
        }

        /// <summary>
        /// Возвращает все элементы списка.
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllItems() 
        {
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            IEnumerable<string> fields = GetFieldNames();
            ListItemCollection listItems = GetItemsFromSPList(query, fields);
            List<T> items = CreateFromSPItems(listItems);

            return items;
        }

        /// <summary>
        /// Возвращает элементы списка удовлетворяющие запросу.
        /// </summary>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.</param>
        /// <returns></returns>
        public List<T> GetItems(IQuery query)
        {
            ListItemCollection listItems = GetItemsFromSPList(query);
            List<T> items = CreateFromSPItems(listItems);

            return items;
        }

        /// <summary>
        /// Возвращает элемент из списка.
        /// </summary>
        /// <param name="id">Идентификатор требуемого элемента.</param>
        /// <returns></returns>
        /// <exception cref="CFT.ToolBelt.SharePointListsContext.ItemNotExistsException">Возникает, если элемент с указанным идентификатором не найден.</exception>
        public T GetItem(int id)
        {
            IQuery query = Camlex.Query().Where(x => (int)x["ID"] == id).Take(1);
            ListItem listItem = GetItemsFromSPList(query).FirstOrDefault();
            if(listItem == null)
            {
                throw new ItemNotExistsException($"Item with ID = {id} not fount.");
            }
            T item = CreateItem(listItem);

            return item;
        }

        /// <summary>
        /// Возвращает элемент из списка, находящийся перед элементом с указанным идентификатором.        
        /// </summary>
        /// <param name="id">Идентификатор элемента, находящегося сразу после требуемого.</param>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery. 
        /// Позволяет задать сортировку, или отфильтровать элементы списка перед определением предыдущего элемента.</param>
        /// <returns> </returns>
        /// <exception cref="CFT.ToolBelt.SharePointListsContext.ItemNotExistsException">Возникает, если элемент с указанным идентификатором является первым элементом списка.</exception>
        public T GetPreviousItem(IQuery query, int id)
        {
            List<int> identifiers = GetIdentifiers(query);
            int position = identifiers.IndexOf(identifiers.First(i => i == id));
            if(position == 0)
            {
                throw new ItemNotExistsException("Item have no previous item.");
            }
            return GetItem(identifiers[position - 1]);
        }

        /// <summary>
        /// Возвращает элемент из списка, находящийся после элемента с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор элемента, находящегося сразу перед требуемым.</param>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.
        /// Позволяет задать сортировку, или отфильтровать элементы списка перед определением следующего элемента.</param>
        /// <returns></returns>
        /// <exception cref="CFT.ToolBelt.SharePointListsContext.ItemNotExistsException">Возникает, если элемент с указанным идентификатором является последним элементом списка.</exception>
        public T GetNextItem(IQuery query, int id)
        {
            List<int> identifiers = GetIdentifiers(query);
            int position = identifiers.IndexOf(identifiers.First(i => i == id));
            if (position == identifiers.Count - 1)
            {
                throw new ItemNotExistsException("Item have no next item.");
            }
            return GetItem(identifiers[position + 1]);
        }
        
        /// <summary>
        /// Добавляет элемент в список. Возвращает идентификатор созданного элемента.
        /// </summary>
        /// <param name="item">Добавляемый элемент.</param>
        /// <returns>Идентификатор созданного элемента.</returns>
        public int AddItem(T item)
        {
            List list = context.Web.Lists.GetByTitle(listName);
            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem newListItem = list.AddItem(itemCreateInfo);
            
            IEnumerable<PropertyInfo> properties = GetAllowedProperties();

            SetListItemFields(newListItem, item, properties);

            newListItem.Update();
            context.ExecuteQuery();
            return newListItem.Id;
        }

        /// <summary>
        /// Удаляет элемент списка.
        /// </summary>
        /// <param name="itemId">Идентификатор элемента, который следует удалить.</param>
        public void DeleteItem(int itemId)
        {
            List list = context.Web.Lists.GetByTitle(listName);
            ListItem itemToDelete = list.GetItemById(itemId);
            itemToDelete.DeleteObject();
            context.ExecuteQuery();            
        }

        /// <summary>
        /// Удаляет элементы списка, удовлетворяющие запросу.
        /// </summary>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.</param>
        /// <returns>Количество удаленных элементов.</returns>
        /// <exception cref="Microsoft.SharePoint.Client.ServerException">
        /// Возникает когда запрос использует слишком много ресурсов.
        /// Т.е. запросу query соответствует слишком много элементов.
        /// </exception>
        public int DeleteItems(IQuery query)
        {
            CamlQuery camlQuery = query.ToCamlQuery();
            IEnumerable<string> fields = new List<string>() { "ID" };
            ListItemCollection itemsToDelete = GetItemsFromSPList(camlQuery, fields);
            int itemsCount = itemsToDelete.Count;
            foreach(ListItem item in itemsToDelete.ToList()) //Приведение к списку обязательно, иначе при удалении может возникнуть исключение, т.к. коллекция при удалении элемента изменится и итерация будет невозможна.
            {
                item.DeleteObject();
            }

            context.ExecuteQuery();
            return itemsCount;
        }

        /// <summary>
        /// Обновляет элемент в списке.
        /// </summary>
        /// <param name="item">Изменяемый элемент, должен содержать поле id.</param>
        public void UpdateItem(T item)
        {            
            IEnumerable<PropertyInfo> properties = GetAllowedProperties();
            PropertyInfo idProperty = properties.First(p => string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase));
            int id = Convert.ToInt32(idProperty.GetValue(item));

            List list = context.Web.Lists.GetByTitle(listName);

            // результат выполнения этого метода кэшируется и при повторном вызове возникает ошибка Version Conflict
            //ListItem itemToUpdate = list.GetItemById(id);

            IQuery queryById = Camlex.Query().Where(x => (int)x["ID"] == id).Take(1);
            ListItemCollection items = list.GetItems(queryById.ToCamlQuery());
            IEnumerable<string> fields = GetFieldNames();
            var retrievals = CreateExpressionFromArray(fields);
            context.Load(items, includes => includes.Include(retrievals));
            context.ExecuteQuery();            

            if (items.Count == 0)
            {
                throw new ItemNotExistsException($"Item with ID = {id} not fount.");
            }
            else
            {
                ListItem itemToUpdate = items.First();
                SetListItemFields(itemToUpdate, item, properties);

                itemToUpdate.Update();
                context.ExecuteQuery();
            }
        }

        /// <summary>
        /// Возвращает коллекцию вложений для указанного элемента списка.
        /// </summary>
        /// <param name="id">Идентификатор элемента списка.</param>
        /// <returns></returns>
        public List<SharePointAttachment> GetAttachments(int id)
        {
            List<SharePointAttachment> attachments = new List<SharePointAttachment>();
                        
            string url = string.Format("{0}/Lists/{1}/Attachments/{2}", context.Url, listName, id);

            try
            {
                Folder attachmentsFolder = context.Web.GetFolderByServerRelativeUrl(url);

                context.Load(attachmentsFolder);
                FileCollection files = attachmentsFolder.Files;
                context.Load(files);
                context.ExecuteQuery(); //Если вложений нет, в это момент может возникнуть исключительная ситуация, которая будет перехвачена. 

                foreach(Microsoft.SharePoint.Client.File file in files)
                {
                    SharePointAttachment attachment = new SharePointAttachment(context);
                    attachment.Name = file.Name;
                    attachment.ServerRelativeUrl = file.ServerRelativeUrl;
                    attachment.Length = file.Length;

                    attachments.Add(attachment);
                }                
            }
            catch (Microsoft.SharePoint.Client.ServerException ex)
            {
                //Вложений может и не быть, это нормальная ситуация. В остальных случаях кидаем исключение выше.
                if (ex.ServerErrorTypeName != "System.IO.FileNotFoundException")
                {
                    throw;
                }
            }
            return attachments;
        }

        /// <summary>
        /// Добавляет вложение к элементу списка.
        /// </summary>
        /// <param name="id">Идентификатор элемента в списке SharePoint.</param>
        /// <param name="fileName">Имя вложения.</param>
        /// <param name="data">Поток данных вложения.</param>
        public void AddAttachment(int id, string fileName, Stream data)
        {
            List list = context.Web.Lists.GetByTitle(listName);
            ListItem item = list.GetItemById(id);
            AttachmentCreationInformation attachFileInfo = new AttachmentCreationInformation();            
            attachFileInfo.ContentStream = data;
            attachFileInfo.FileName = fileName;
            Attachment attachment = item.AttachmentFiles.Add(attachFileInfo);
            context.Load(attachment);
            context.ExecuteQuery();
        }

        /// <summary>
        /// Возвращает количество элементов, удовлетворяющих запросу.
        /// </summary>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.</param>
        /// <returns></returns>
        public int Count(IQuery query)
        {            
            CamlQuery camlQuery = query.ToCamlQuery();
            IEnumerable<string> fields = new[] { "ID" };            
            var retrievals = CreateExpressionFromArray(fields);

            List list = context.Web.Lists.GetByTitle(listName);
            ListItemCollection items = list.GetItems(camlQuery);
            context.Load(items, includes => includes.Include(retrievals));
            context.ExecuteQuery();

            return items.Count();
        }

        /// <summary>
        /// Удаляет все элементы списка.
        /// Возвращает количество удаленных элементов.
        /// </summary>
        /// <returns></returns>
        public int Clear()
        {
            List list = context.Web.Lists.GetByTitle(listName);
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection items = list.GetItems(query);

            context.Load
            (
                items,
                eachItem => eachItem.Include
                (
                    item => item,
                    item => item["ID"]
                )
            );

            context.ExecuteQuery();

            int totalItems = items.Count;
            int itemsInQuery = 0;

            if (totalItems > 0)
            {
                for (int counter = totalItems - 1; counter > -1; counter--)
                {
                    items[counter].DeleteObject();
                    itemsInQuery++;
                    if (itemsInQuery == maxItemsInDeleteQuery)
                    {
                        context.ExecuteQuery();
                        itemsInQuery = 0;
                    }
                }

                if (itemsInQuery != 0)
                {
                    context.ExecuteQuery();
                }
            }
            return totalItems;
        }

        /// <summary>
        /// Ограничивает число запрашиваемых из SharePoint полей, указанными полями.
        /// </summary>
        /// <param name="expr">Свойства, для которых надо получить поля списка SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> IncludeFields(Expression<Func<T, object>> expr)
        {
            string[] properties = GetNamesFromExpression(expr);
            return IncludeFields(properties);
        }

        /// <summary>
        /// Ограничивает число запрашиваемых из SharePoint полей, путем исключения указанных полей.
        /// </summary>
        /// <param name="expr">Свойства, для которых не надо получать поля списка SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> ExcludeFields(Expression<Func<T, object>> expr)
        {
            string[] properties = GetNamesFromExpression(expr);
            return ExcludeFields(properties);
        }

        /// <summary>
        /// Ограничивает число запрашиваемых из SharePoint полей, указанными полями.
        /// </summary>
        /// <param name="properties">Имена свойств, для которых надо получить поля списка SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> IncludeFields(params string[] properties)
        {
            var clone = (SharePointList<T>)this.MemberwiseClone();
            clone.allowedPropertyNames = properties.ToList();
            return clone;
        }

        /// <summary>
        /// Ограничивает число запрашиваемых из SharePoint полей, путем исключения указанных полей.
        /// </summary>
        /// <param name="properties">Имена свойств, для которых не надо получать поля списка SharePoint.</param>
        /// <returns></returns>
        public SharePointList<T> ExcludeFields(params string[] properties)
        {
            var clone = (SharePointList<T>)this.MemberwiseClone();
            clone.disAllowedPropertyNames = properties.ToList();
            return clone;
        }

        /// <summary>
        /// Возвращает хэш код.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 19;
            int primeNumber = 31; //Multiplication by 31 is easily optimizable to a shift and a subtraction, which makes it attractive.
            unchecked
            {
                hash = hash * primeNumber + listName.GetHashCode();

                if (allowedPropertyNames != null)
                {
                    hash = hash * primeNumber + allowedPropertyNames.GetCustomHashCode();
                }

                // Если это не сделать то результат будет равен для allowedPropertyNames == disAllowedPropertyNames
                hash = hash * primeNumber; 

                if (disAllowedPropertyNames != null)
                {
                    hash = hash * primeNumber + disAllowedPropertyNames.GetCustomHashCode();
                }
            }
            
            return hash;
        }

        /// <summary>
        /// Определяет равен ли указанный объект текущему.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }                
            if (obj == null)
            {
                return false;
            }
            if(!(obj is SharePointList<T>))
            {
                return false;
            }

            var other = (SharePointList<T>)obj;

            return this.GetHashCode() == other.GetHashCode();
        }

        /// <summary>
        /// Возвращает элементы списка, удовлетворяющие запросу.
        /// </summary>        
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.</param>
        /// <returns></returns>
        protected ListItemCollection GetItemsFromSPList(IQuery query)
        {
            CamlQuery camlQuery = query.ToCamlQuery();
            IEnumerable<string> fields = GetFieldNames();
            return GetItemsFromSPList(camlQuery, fields);
        }

        /// <summary>
        /// Возвращает элементы списка удовлетворяющие запросу.
        /// </summary>        
        /// <param name="query">Запрос, в виде объекта типа Microsoft.SharePoint.Client.CamlQuery.</param>
        /// <param name="fields">Список полей, которые надо вернуть из списка SharePoint.</param>
        /// <returns></returns>
        protected ListItemCollection GetItemsFromSPList(CamlQuery query, IEnumerable<string> fields)
        {            
            var retrievals = CreateExpressionFromArray(fields);
            query.DatesInUtc = false; //Без этого, возвращаемые даты были в utc.
            List list = context.Web.Lists.GetByTitle(listName);
            ListItemCollection items = list.GetItems(query);
            context.Load(items, includes => includes.Include(retrievals));
            context.ExecuteQuery();
            return items;
        }

        /// <summary>
        /// Возвращает список, созданный на основе коллекции элементов списка SharePoint.
        /// </summary>        
        /// <param name="items">Коллекция элементов списка SharePoint.</param>        
        /// <returns></returns>
        protected List<T> CreateFromSPItems(ListItemCollection items)
        {
            List<T> result = new List<T>();

            foreach (var item in items)
            {
                result.Add(CreateItem(item));
            }

            return result;
        }

        private T CreateItem(ListItem listItem)
        {            
            T newItem = new T();            
            IEnumerable<PropertyInfo> properties = GetAllowedProperties();
            foreach (PropertyInfo property in properties)
            {
                object fieldValue = GetFieldValue(listItem, property);                

                Type type = property.PropertyType;

                if (type.IsValueType)
                {
                    property.SetValueType(newItem, fieldValue);
                }
                else if (type.IsGenericType && type.GetInterfaces().Contains(typeof(IEnumerable<SharePointAttachment>)))
                {
                    property.SetAttachments(newItem, listItem.AttachmentFiles, context);
                }
                else
                {
                    property.SetReferenceType(newItem, fieldValue);
                }
            }

            return newItem;
        }

        private object GetFieldValue(ListItem listItem, PropertyInfo property)
        {
            object fieldValue = null;

            string fieldName = property.GetSPFieldName();

            try
            {
                if (Attribute.IsDefined(property, typeof(SharePointFieldNameAttribute)))
                {

                }

                if (Attribute.IsDefined(property, typeof(LookupIdAttribute)))
                {
                    if (property.PropertyType.IsArray)
                    {
                        fieldValue = ((FieldLookupValue[])listItem[fieldName]).Select(x => x.LookupId);
                    }
                    else
                    {
                        if (!(listItem[fieldName] == null && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            fieldValue = ((FieldLookupValue)listItem[fieldName]).LookupId; // необязательное ссылочное поле может принимать значение null.
                        }
                    }
                }
                else if (Attribute.IsDefined(property, typeof(LookupValueAttribute)))
                {
                    if (property.PropertyType.IsArray)
                    {
                        fieldValue = ((FieldLookupValue[])listItem[fieldName]).Select(x => x.LookupValue);
                    }
                    else
                    {
                        if (listItem[fieldName] != null) //ссылочное поле может быть необязательным.
                        {
                            fieldValue = ((FieldLookupValue)listItem[fieldName]).LookupValue;
                        }
                    }
                }
                else if (Attribute.IsDefined(property, typeof(SharePointUrlFieldPathAttribute)))
                {
                    fieldValue = listItem[fieldName];

                    if (fieldValue != null)
                    {
                        SharePointUrlField spUrlField = new SharePointUrlField(((FieldUrlValue)fieldValue).Url, string.Empty);
                        fieldValue = spUrlField.Path;
                    }
                }
                else if (string.Equals(fieldName, attachmentsFieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    //fieldValue = listItem.AttachmentFiles;
                }
                else
                {
                    fieldValue = listItem[fieldName];
                }
            }
            catch(NullReferenceException ex)
            {
                throw new NullReferenceException(ex.Message + $" Свойство: {fieldName}");
            }

            return fieldValue;
        }

        private void SetListItemFields(ListItem listItem, object item, IEnumerable<PropertyInfo> properties)
        {
            foreach (PropertyInfo property in properties)
            {
                string fieldName = property.GetSPFieldName();

                if (string.Equals(fieldName, "ID", StringComparison.OrdinalIgnoreCase)) //При создании элемента идентификатор будет назначен автоматически.
                {
                    continue;
                }

                Type propertyType = property.PropertyType;
                object propertyValue = property.GetValue(item);

                if (Attribute.IsDefined(property, typeof(LookupIdAttribute)))
                {
                    if (propertyType.IsArray) //Для множественных значений.
                    {
                        int[] values = (int[])propertyValue;
                        listItem[fieldName] = CreateLookupCollection(values);
                    }
                    else
                    {                        
                        if (propertyValue == null && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            listItem[fieldName] = null; // необязательное ссылочное поле может принимать значение null.
                        }
                        else
                        {
                            listItem[fieldName] = CreateLookup(propertyValue);
                        }                        
                    }
                }
                else if (Attribute.IsDefined(property, typeof(LookupValueAttribute)))
                {
                    //TODO: Не зная идентификатора создать ссылочное поле невозможно, что с этим делать пока не понятно
                    throw new NotImplementedException();
                }
                else if (propertyType == typeof(SharePointLookupField))
                {                    
                    if (propertyValue == null)
                    {
                        listItem[fieldName] = null; // необязательное ссылочное поле может принимать значение null.
                    }
                    else
                    {                        
                        listItem[fieldName] = CreateLookup(((SharePointLookupField)propertyValue).LookupId);
                    }
                }
                else if (propertyType == typeof(SharePointLookupField[]))
                {   //Для множественных значений.
                    SharePointLookupField[] values = (SharePointLookupField[])propertyValue;
                    listItem[fieldName] = CreateLookupCollection(values.Select(v => v.LookupId));
                }
                else if(propertyType == typeof(SharePointUrlField))
                {
                    SharePointUrlField value = (SharePointUrlField)propertyValue;
                    listItem[fieldName] = CreateFieldUrl(value);                    
                }
                else
                {
                    listItem[fieldName] = propertyValue;
                }

            }
        }

        private List<string> GetFieldNames()
        {
            List<string> fieldNames = new List<string>();
            
            IEnumerable<PropertyInfo> properties = GetAllowedProperties();

            foreach (PropertyInfo property in properties)
            {
                string fieldName = property.GetSPFieldName();
                fieldNames.Add(fieldName);
            }

            return fieldNames;
        }        

        /// <summary>
        /// Возвращает коллекцию идентификаторов всех элементов списка.
        /// </summary>
        /// <param name="query">Запрос в виде объекта, реализующего интерфейс CamlexNET.Interfaces.IQuery.
        /// Позволяет задать сортировку, или отфильтровать элементы списка.
        /// ViewFields задавать не следует т.к. возвращаются только идентификаторы.</param>
        /// <returns></returns>
        private List<int> GetIdentifiers(IQuery query)
        {
            List<int> identifiers = new List<int>();
            CamlQuery camlQuery = query.ToCamlQuery();
            IEnumerable<string> fields = new[] { "ID" };
            ListItemCollection listItems = GetItemsFromSPList(camlQuery, fields);
            foreach (var item in listItems)
            {
                identifiers.Add(Convert.ToInt32(item["ID"]));
            }

            return identifiers;
        }

        
        private Expression<Func<ListItem, object>>[] CreateExpressionFromArray(IEnumerable<string> fieldNames)
        {
            List<Expression<Func<ListItem, object>>> fieldsExpressions = new List<Expression<Func<ListItem, object>>>();
            ParameterExpression parameter = Expression.Parameter(typeof(ListItem), "i");

            foreach (string fieldName in fieldNames)
            {
                Expression expression = null;                

                if (string.Equals(fieldName, attachmentsFieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    expression = Expression.PropertyOrField(parameter, fieldName);
                }
                else
                {
                    expression = Expression.Call(parameter,
                        typeof(ListItem).GetMethod("get_Item", new[] { typeof(string) }),
                        new[] { Expression.Constant(fieldName) });
                }

                Expression<Func<ListItem, object>> lambdaExpression = Expression.Lambda<Func<ListItem, object>>(
                expression,
                Expression.Parameter(typeof(ListItem), "i"));

                fieldsExpressions.Add(lambdaExpression);
            }

            Expression<Func<ListItem, object>>[] fieldsExpressionsArray = fieldsExpressions.ToArray();

            return fieldsExpressionsArray;
        }

        private FieldLookupValue CreateLookup(Object itemId)
        {
            return CreateLookup(Convert.ToInt32(itemId));
        }

        private FieldLookupValue CreateLookup(int itemId)
        {
            FieldLookupValue fieldLookup = new FieldLookupValue();
            fieldLookup.LookupId = itemId;
            return fieldLookup;
        }

        private FieldLookupValue[] CreateLookupCollection(IEnumerable<int> itemIds)
        {
            List<FieldLookupValue> fieldLookups = new List<FieldLookupValue>();
            foreach(int id in itemIds)
            {
                fieldLookups.Add(CreateLookup(id));
            }
            return fieldLookups.ToArray();
        }

        private FieldUrlValue CreateFieldUrl(SharePointUrlField value)
        {
            FieldUrlValue fieldUrl = new FieldUrlValue();
            fieldUrl.Url = value.Url;
            fieldUrl.Description = value.Description;
            return fieldUrl;
        }        
        
        private IEnumerable<PropertyInfo> GetAllowedProperties()
        {
            Type selfType = typeof(T);
            // Берутся только свойства у которых есть set метод даже если он приватный.
            // Не берутся свойства, помеченные специальным атрибутом.
            IEnumerable<PropertyInfo> properties = selfType
                .GetProperties()
                .Where(p => p.CanWrite && p.GetCustomAttributes(typeof(SharePointNotMappedAttribute)).Count() == 0);

            // Ограничить передаваемые свойства можно еще с помощью специальных методов Include и Exclude
            // Которые задают поле allowedPropertyNames
            if (allowedPropertyNames != null && allowedPropertyNames.Any())
            {
                properties = properties.Where(p => allowedPropertyNames.Contains(p.Name));
            }

            if (disAllowedPropertyNames != null && disAllowedPropertyNames.Any())
            {
                properties = properties.Where(p => !disAllowedPropertyNames.Contains(p.Name));
            }

            return properties;
        }

        private string[] GetNamesFromExpression(Expression<Func<T, object>> expr)
        {
            var x = ((NewExpression)expr.Body).Members;
            string[] names = x.Select(m => m.Name).ToArray();
            return names;
        }
    }
