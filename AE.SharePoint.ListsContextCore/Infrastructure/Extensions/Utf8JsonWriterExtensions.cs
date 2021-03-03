using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AE.SharePoint.ListsContextCore.Infrastructure.Extensions
{
    internal static class Utf8JsonWriterExtensions
    {
        public static void Write(this Utf8JsonWriter writer, Object obj, ListItemPropertyCreationInfo propertyCreationInfo)
        {            
            TypeCode typeCode = Type.GetTypeCode(propertyCreationInfo.PropertyToSet.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Int32:
                    int int32Value = (int)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteNumber(propertyCreationInfo.SharePointFieldName, int32Value);
                    break;
                case TypeCode.Int64:
                    long int64Value = (long)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteNumber(propertyCreationInfo.SharePointFieldName, int64Value);
                    break;
                case TypeCode.Double:
                    int doubleValue = (int)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteNumber(propertyCreationInfo.SharePointFieldName, doubleValue);
                    break;
                case TypeCode.Decimal:
                    int decimalValue = (int)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteNumber(propertyCreationInfo.SharePointFieldName, decimalValue);
                    break;
                case TypeCode.Boolean:
                    bool boolValue = (bool)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteBoolean(propertyCreationInfo.SharePointFieldName, boolValue);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTimeValue = (DateTime)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    string dateTimeString = dateTimeValue.ToString(); //TODO: Уточнить преобразование.
                    writer.WriteString(propertyCreationInfo.SharePointFieldName, dateTimeString);
                    break;
                case TypeCode.String:
                    string stringValue = (string)propertyCreationInfo.PropertyToSet.GetValue(obj);
                    writer.WriteString(propertyCreationInfo.SharePointFieldName, stringValue);
                    break;
                default:
                    //ThrowNotImplementedException(type); TODO: Сделать исключение.
                    break;
            }
            
        }

        public static void WriteSharePointUrlFieldObject(this Utf8JsonWriter writer, Object obj, ListItemPropertyCreationInfo propertyCreationInfo)
        {
            writer.WriteStartObject(propertyCreationInfo.SharePointFieldName);
            var sharePointUrlFieldValue = (SharePointUrlField)propertyCreationInfo.PropertyToSet.GetValue(obj);
            if (sharePointUrlFieldValue != null)
            {
                writer.WriteString("Url", sharePointUrlFieldValue.Url);
                writer.WriteString("Description", sharePointUrlFieldValue.Description);
            }
            writer.WriteEndObject();
        }
    }
}
