using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class SharePointJsonConverter : IConverter
    {
        private readonly List<ListItemPropertyCreationInfo> propertiesCreationInfo;

        public SharePointJsonConverter(List<ListItemPropertyCreationInfo> propertiesCreationInfo)
        {
            this.propertiesCreationInfo = propertiesCreationInfo;
        }

        public T Convert<T>(object source) where T : new()
        {
            var sourceJson = source as string;
            JsonDocument jsonDocument = JsonDocument.Parse(sourceJson);
            JsonElement jsonItem = jsonDocument.RootElement.GetProperty("d");
            var result = Create<T>(jsonItem);

            return result;
        }

        public List<T> ConvertItems<T>(object source) where T: new()
        {
            var sourceJson = source as string;
            JsonDocument jsonDocument = JsonDocument.Parse(sourceJson);
            JsonElement jsonResults = jsonDocument.RootElement.GetProperty("d").GetProperty("results");
            var jsonItems = jsonResults.EnumerateArray();

            var result = jsonItems
                .Select(i => Create<T>(i))
                .ToList();

            return result;
        }

        private T Create<T>(JsonElement sourceJson) where T : new()
        {
            var newItem = new T();

            foreach(var property in propertiesCreationInfo)
            {
                if(!sourceJson.TryGetProperty(property.SharePointFieldName, out JsonElement jsonField))
                {
                    //TODO: Сделать новый тип для исключения.
                    throw new Exception($"Can`t find SharePoint field for property {property.PropertyToSet.Name}");
                }

                Type type = property.PropertyToSet.PropertyType;

                if (type.IsValueType)
                {
                    SetValueType(newItem, property.PropertyToSet, jsonField);
                }                
                else
                {
                    SetReferenceType(newItem, property.PropertyToSet, jsonField);
                }
            }
            
            return newItem;
        }

        private static void SetValueType<T>(T targetItem, PropertyInfo propertyToSet, JsonElement jsonField)
        {
            Type type = propertyToSet.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (jsonField.ValueKind == JsonValueKind.Null)
                {
                    propertyToSet.SetValue(targetItem, null);
                    return;
                }
                else
                {
                    type = type.GetGenericArguments()[0];
                }
            }
            //TODO: возможно тоже лучше вынести в ListItemPropertyCreationInfo
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int32:
                    // шарепоинт ссылочные поля преобразует в строки, при преобразовании поля типа число у него появляются точка и нули после запятой.
                    //int int32Value = Attribute.IsDefined(property.PropertyToSet, typeof(LookupValueAttribute)) ?
                    //    Convert.ToInt32(value.ToString().Replace(',', '.').Split('.').First()) :
                    //    Convert.ToInt32(value);
                    int int32Value = jsonField.GetInt32();
                    propertyToSet.SetValue(targetItem, int32Value);
                    break;
                case TypeCode.Int64:
                    //long int64Value = Attribute.IsDefined(propertyToSet, typeof(LookupValueAttribute)) ?
                    //    Convert.ToInt64(value.ToString().Replace(',', '.').Split('.').First()) :
                    //    Convert.ToInt64(value);
                    long int64Value = jsonField.GetInt64();
                    propertyToSet.SetValue(targetItem, int64Value);
                    break;
                case TypeCode.Double:
                    double doubleValue = jsonField.GetDouble();
                    propertyToSet.SetValue(targetItem, doubleValue);
                    break;
                case TypeCode.Decimal:
                    decimal decimalValue = jsonField.GetDecimal();
                    propertyToSet.SetValue(targetItem, decimalValue);
                    break;
                case TypeCode.Boolean:
                    bool boolValue = jsonField.GetBoolean();
                    propertyToSet.SetValue(targetItem, boolValue);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTimeValue = jsonField.ValueKind == JsonValueKind.Null ? DateTime.MinValue : jsonField.GetDateTime();
                    propertyToSet.SetValue(targetItem, dateTimeValue);
                    break;
                default:
                    //ThrowNotImplementedException(type); TODO: Сделать исключение.
                    break;
            }
        }

        public static void SetReferenceType<T>(T targetItem, PropertyInfo propertyToSet, JsonElement jsonField)
        {
            Type type = propertyToSet.PropertyType;
            //TODO: а если это всё таки null? нужен какой то признак обязательное ли это поле или нет! атрибут который задает логику.
            
            //TODO: Узнать как тепреь передаются такие типы.
            //if (type == typeof(SharePointLookupField))
            //{
            //    SharePointLookupField spLookupField;
            //    if (value == null)
            //    {
            //        spLookupField = new SharePointLookupField();
            //    }
            //    else
            //    {
            //        FieldLookupValue fieldLookupValue = (FieldLookupValue)value;
            //        spLookupField = new SharePointLookupField(fieldLookupValue.LookupId, fieldLookupValue.LookupValue);
            //    }
            //    propertyToSet.SetValue(instance, spLookupField);
            //}
            //else if (type == typeof(SharePointLookupField[])) //Эта проверка должна идти впереди проверки на тип IsArray
            //{
            //    List<SharePointLookupField> spLookupFields = new List<SharePointLookupField>();
            //    foreach (FieldLookupValue field in (FieldLookupValue[])value)
            //    {
            //        SharePointLookupField spLookupField = new SharePointLookupField(field.LookupId, field.LookupValue);
            //        spLookupFields.Add(spLookupField);
            //    }
            //    propertyToSet.SetValue(instance, spLookupFields.ToArray());
            //}
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                TypeCode elementTypeCode = Type.GetTypeCode(elementType);

                var result = jsonField.EnumerateArray()
                        .Select(o =>  o) //TODO: тут должно быть преобразование.
                        .ToArray();

                //switch (elementTypeCode)
                //{
                //    case TypeCode.Int32:
                //        int[] int32Values = ((IEnumerable<int>)value).ToArray();
                //        propertyToSet.SetValue(instance, int32Values);
                //        break;
                //    case TypeCode.String:
                //        string[] stringValues = ((IEnumerable<string>)value).ToArray();
                //        propertyToSet.SetValue(instance, stringValues);
                //        break;
                //    default:
                //        ThrowNotImplementedException(type);
                //        break;
                //}
            }
            else if (type == typeof(String))
            {
                string stringValue = jsonField.GetString();
                propertyToSet.SetValue(targetItem, stringValue);
            }
            //else if (type == typeof(SharePointUrlField))
            //{
            //    SharePointUrlField spUrlField;
            //    if (value == null)
            //    {
            //        spUrlField = new SharePointUrlField();
            //    }
            //    else
            //    {
            //        FieldUrlValue fieldUrlValue = (FieldUrlValue)value;
            //        spUrlField = new SharePointUrlField(fieldUrlValue.Url, fieldUrlValue.Description);
            //    }
            //    propertyToSet.SetValue(targetItem, spUrlField);
            //}
            else
            {
                //ThrowNotImplementedException(type);
            }
        }

        //public static void SetAttachments(this PropertyInfo propertyToSet, Object instance, AttachmentCollection attachmentCollection, ClientContext context)
        //{
        //    List<SharePointAttachment> attachments = new List<SharePointAttachment>();
        //    foreach (Attachment attachment in attachmentCollection)
        //    {
        //        SharePointAttachment spAttachment = new SharePointAttachment(context);
        //        spAttachment.Name = attachment.FileName;
        //        spAttachment.ServerRelativeUrl = attachment.ServerRelativeUrl;
        //        spAttachment.Length = 0; //тут пока не ясно можно ли вообще узнать размер вложения.
        //        attachments.Add(spAttachment);
        //    }
        //    propertyToSet.SetValue(instance, attachments);
        //}

        //private static void ThrowNotImplementedException(Type type)
        //{
        //    throw new NotImplementedException(string.Format("Не реализовано преобразование поля списка SharePoint для типа данных {0}", type));
        //}
    }
}
