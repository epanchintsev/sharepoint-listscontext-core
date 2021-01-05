using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

using AE.SharePoint.ListsContextCore.Infrastructure.Extensions;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class SharePointJsonConverter : IConverter
    {
        private readonly bool datesFromText;
        private readonly string datesFromTextFormat;

        public SharePointJsonConverter(bool datesFromText, string datesFromTextFormat)
        {
            this.datesFromText = datesFromText;
            this.datesFromTextFormat = datesFromTextFormat;
        }

        public T ConvertFromSPEntity<T>(object source, IEnumerable<ListItemPropertyCreationInfo> properties) where T : new()
        {
            var sourceJson = source as string;
            JsonDocument jsonDocument = JsonDocument.Parse(sourceJson);
            JsonElement jsonItem = jsonDocument.RootElement.GetProperty("d");
            var result = CreateObject<T>(jsonItem, properties);

            return result;
        }

        public List<T> ConvertFromSPEntities<T>(object source, IEnumerable<ListItemPropertyCreationInfo> properties) where T: new()
        {
            var sourceJson = source as string;
            JsonDocument jsonDocument = JsonDocument.Parse(sourceJson);
            JsonElement jsonResults = jsonDocument.RootElement.GetProperty("d").GetProperty("results");
            var jsonItems = jsonResults.EnumerateArray();

            var result = jsonItems
                .Select(i => CreateObject<T>(i, properties))
                .ToList();

            return result;
        }

        public string ConvertToSPEntity<T>(Object source, string sharePointTypeName, IEnumerable<ListItemPropertyCreationInfo> properties)
        {
            return CreateJson(source, sharePointTypeName, properties);
        }

        private string DateTimeFormat => datesFromText ? datesFromTextFormat : string.Empty;

        private T CreateObject<T>(JsonElement sourceJson, IEnumerable<ListItemPropertyCreationInfo> properties) where T : new()
        {
            var newItem = new T();

            foreach(var property in properties)
            {
                JsonElement source = IsDateFormText(property) ?
                    sourceJson.GetProperty("FieldValuesAsText"):
                    sourceJson;
                
                if(!source.TryGetProperty(property.SharePointFieldName, out JsonElement jsonField))
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

        private string CreateJson<T>(T sourceObject, string sharePointTypeName, IEnumerable<ListItemPropertyCreationInfo> properties)
        {
            string resultJson;

            using (var ms = new MemoryStream())
            using (var writer = new Utf8JsonWriter(ms))
            {
                writer.WriteStartObject();

                writer.WriteStartObject("__metadata");
                writer.WriteString("type", sharePointTypeName);
                writer.WriteEndObject();

                foreach (var property in properties)
                {
                    Type type = property.PropertyToSet.PropertyType;

                    if (type.IsValueType)
                    {
                        writer.Write(sourceObject, property);
                    }
                    else
                    {
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
                            //Type elementType = type.GetElementType();
                            //TypeCode elementTypeCode = Type.GetTypeCode(elementType);

                            //var result = jsonField.EnumerateArray()
                            //        .Select(o => o) //TODO: тут должно быть преобразование.
                            //        .ToArray();

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
                            writer.Write(sourceObject, property);
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
                }

                writer.WriteEndObject();
                writer.Flush();

                resultJson = Encoding.UTF8.GetString(ms.ToArray());
            }

            return resultJson;
        }

        private void SetValueType<T>(T targetItem, PropertyInfo propertyToSet, JsonElement jsonField)
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

            propertyToSet.SetValueFromJson(targetItem, jsonField, DateTimeFormat);            
        }

        public void SetReferenceType<T>(T targetItem, PropertyInfo propertyToSet, JsonElement jsonField)
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
                propertyToSet.SetValueFromJson(targetItem, jsonField, DateTimeFormat);
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

        private bool IsDateFormText(ListItemPropertyCreationInfo propertyCreationInfo)
        {
            var isDateFromText = datesFromText && Type.GetTypeCode(propertyCreationInfo.PropertyToSet.PropertyType) == TypeCode.DateTime;
            return isDateFromText;
        }
    }
}
