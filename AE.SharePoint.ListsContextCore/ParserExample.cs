internal static class PropertyInfoExtensions
    {
        /// <summary>
        /// Устанавливает значение свойства, являющееся значимым типом, при этом приводит значение к требуемому типу.
        /// </summary>
        /// <param name="propertyToSet">Свойство объекта, которое будет установлено.</param>
        /// <param name="instance">Объект, значение свойства которого будет установлено.</param>
        /// <param name="value">Значение, которое надо задать свойству.</param>
        public static void SetValueType(this PropertyInfo propertyToSet, object instance,  object value)
        {
            Type type = propertyToSet.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    propertyToSet.SetValue(instance, null);
                    return;
                }
                else
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int32:
                    // шарепоинт ссылочные поля преобразует в строки, при преобразовании поля типа число у него появляются точка и нули после запятой.
                    int int32Value = Attribute.IsDefined(propertyToSet, typeof(LookupValueAttribute)) ?
                        Convert.ToInt32(value.ToString().Replace(',', '.').Split('.').First()) :
                        Convert.ToInt32(value);
                    propertyToSet.SetValue(instance, int32Value);
                    break;
                case TypeCode.Int64:
                    long int64Value = Attribute.IsDefined(propertyToSet, typeof(LookupValueAttribute)) ?
                        Convert.ToInt64(value.ToString().Replace(',', '.').Split('.').First()) :
                        Convert.ToInt64(value);
                    propertyToSet.SetValue(instance, int64Value);
                    break;
                case TypeCode.Double:
                    double doubleValue = Convert.ToDouble(value);
                    propertyToSet.SetValue(instance, doubleValue);
                    break;
                case TypeCode.Decimal:
                    decimal decimalValue = Convert.ToDecimal(value);
                    propertyToSet.SetValue(instance, decimalValue);
                    break;
                case TypeCode.Boolean:
                    bool boolValue = Convert.ToBoolean(value);
                    propertyToSet.SetValue(instance, boolValue);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTimeValue = value == null ? DateTime.MinValue : Convert.ToDateTime(value);
                    propertyToSet.SetValue(instance, dateTimeValue);
                    break;
                default:
                    ThrowNotImplementedException(type);
                    break;
            }
        }

        /// <summary>
        /// Устанавливает значение свойства, являющееся ссылочным типом, заданного экземпляра.
        /// </summary>
        /// <param name="propertyToSet">Свойство объекта, которое будет установлено.</param>
        /// <param name="instance">Объект, значение свойства которого будет установлено.</param>
        /// <param name="value">Значение, которое надо задать свойству.</param>
        public static void SetReferenceType(this PropertyInfo propertyToSet, object instance,  object value)
        {
            Type type = propertyToSet.PropertyType;
            //TODO: а если это всё таки null? нужен какой то признак обязательное ли это поле или нет! атрибут который задает логику.
            if (type == typeof(SharePointLookupField))
            {
                SharePointLookupField spLookupField;
                if (value == null)
                {
                    spLookupField = new SharePointLookupField();
                }
                else
                {
                    FieldLookupValue fieldLookupValue = (FieldLookupValue)value;
                    spLookupField = new SharePointLookupField(fieldLookupValue.LookupId, fieldLookupValue.LookupValue);
                }
                propertyToSet.SetValue(instance, spLookupField);
            }
            else if (type == typeof(SharePointLookupField[])) //Эта проверка должна идти впереди проверки на тип IsArray
            {
                List<SharePointLookupField> spLookupFields = new List<SharePointLookupField>();
                foreach (FieldLookupValue field in (FieldLookupValue[])value)
                {
                    SharePointLookupField spLookupField = new SharePointLookupField(field.LookupId, field.LookupValue);
                    spLookupFields.Add(spLookupField);
                }
                propertyToSet.SetValue(instance, spLookupFields.ToArray());
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                TypeCode elementTypeCode = Type.GetTypeCode(elementType);

                switch (elementTypeCode)
                {
                    case TypeCode.Int32:
                        int[] int32Values = ((IEnumerable<int>)value).ToArray();
                        propertyToSet.SetValue(instance, int32Values);
                        break;
                    case TypeCode.String:
                        string[] stringValues = ((IEnumerable<string>)value).ToArray();
                        propertyToSet.SetValue(instance, stringValues);
                        break;
                    default:
                        ThrowNotImplementedException(type);
                        break;
                }
            }
            else if (type == typeof(String))
            {
                string stringValue = Convert.ToString(value);
                propertyToSet.SetValue(instance, stringValue);
            }
            else if (type == typeof(SharePointUrlField))
            {
                SharePointUrlField spUrlField;
                if (value == null)
                {
                    spUrlField = new SharePointUrlField();
                }
                else
                {
                    FieldUrlValue fieldUrlValue = (FieldUrlValue)value;
                    spUrlField = new SharePointUrlField(fieldUrlValue.Url, fieldUrlValue.Description);
                }
                propertyToSet.SetValue(instance, spUrlField);
            }
            else
            {
                ThrowNotImplementedException(type);
            }
        }

        public static void SetAttachments(this PropertyInfo propertyToSet, Object instance,  AttachmentCollection attachmentCollection, ClientContext context)
        {
            List<SharePointAttachment> attachments = new List<SharePointAttachment>();
            foreach (Attachment attachment in attachmentCollection)
            {
                SharePointAttachment spAttachment = new SharePointAttachment(context);
                spAttachment.Name = attachment.FileName;
                spAttachment.ServerRelativeUrl = attachment.ServerRelativeUrl;
                spAttachment.Length = 0; //тут пока не ясно можно ли вообще узнать размер вложения.
                attachments.Add(spAttachment);
            }
            propertyToSet.SetValue(instance, attachments);
        }

        /// <summary>
        /// Возвращает название поля списка SP соответствующее данному свойству.
        /// Если указан атрибут SharePointFieldNameAttribute в качестве названия поля берется его значение,
        /// в противном случае название поля списка SP совпадает с названием свойства.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetSPFieldName(this PropertyInfo property)
        {
            var fieldNameAttribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is SharePointFieldNameAttribute);
            string fieldName = fieldNameAttribute != null ? ((SharePointFieldNameAttribute)fieldNameAttribute).Name : property.Name;
            return fieldName;
        }

        private static void ThrowNotImplementedException(Type type)
        {
            throw new NotImplementedException(string.Format("Не реализовано преобразование поля списка SharePoint для типа данных {0}", type));
        }
    }
