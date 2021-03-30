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
                JsonElement source = GetJsonSource(sourceJson, property);
                string jsonFieldName = GetJsonFieldName(property);

                if (!source.TryGetProperty(jsonFieldName, out JsonElement jsonField))
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
                        
                        if (type == typeof(String))
                        {
                            writer.Write(sourceObject, property);
                        }
                        else if (type == typeof(SharePointUrlField))
                        {
                            writer.WriteSharePointUrlFieldObject(sourceObject, property);
                        }
                        else
                        {
                            ThrowNotImplementedException(type);
                        }
                    }
                }

                writer.WriteEndObject();
                writer.Flush();

                resultJson = Encoding.UTF8.GetString(ms.ToArray());
            }

            return resultJson;
        }        

        public void SetReferenceType<T>(T targetItem, PropertyInfo propertyToSet, JsonElement jsonField)
        {
            if(jsonField.ValueKind == JsonValueKind.Null)
            {
                return;
            }            
            
            Type type = propertyToSet.PropertyType;             
            
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                TypeCode elementTypeCode = Type.GetTypeCode(elementType);

                var result = jsonField.EnumerateArray()
                    .Select(o =>  o) //TODO: тут должно быть преобразование.
                    .ToArray();
                
            }
            else if (type == typeof(String))
            {
                propertyToSet.SetValueFromJson(targetItem, jsonField, DateTimeFormat);
            }
            else if (type == typeof(SharePointUrlField))
            {
                propertyToSet.SetSharePointUrlFieldFromJson(targetItem, jsonField);
            }
            else
            {
                ThrowNotImplementedException(type);
            }
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

        private bool IsDateFormText(ListItemPropertyCreationInfo propertyCreationInfo)
        {
            var isDateFromText = datesFromText && Type.GetTypeCode(propertyCreationInfo.PropertyToSet.PropertyType) == TypeCode.DateTime;
            return isDateFromText;
        }

        private JsonElement GetJsonSource(JsonElement sourceJson, ListItemPropertyCreationInfo property)
        {
            if(IsDateFormText(property))
            {
                return sourceJson.GetProperty("FieldValuesAsText");
            }
            
            if(property.SharePointFieldType == SharePointFieldType.LookupValue)
            {
                return sourceJson.GetProperty(property.SharePointFieldName);
            }            

            return sourceJson;
        }

        private static string GetJsonFieldName(ListItemPropertyCreationInfo property)
        {
            string fieldName;
            switch(property.SharePointFieldType)
            {
                case SharePointFieldType.LookupId:                    
                    fieldName = $"{property.SharePointFieldName}Id";
                    break;
                case SharePointFieldType.LookupValue:
                    fieldName = (string)property.AdditionalData;
                    break;
                default:
                    fieldName = property.SharePointFieldName;
                    break;
            }

            return fieldName;
        }
        
        private static void ThrowNotImplementedException(Type type)
        {
            throw new NotImplementedException(string.Format("Converter for type {0} not implemented.", type));
        }
    }
}
