using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace AE.SharePoint.ListsContextCore.Infrastructure.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static void SetValueFromJson(this PropertyInfo property, Object obj, JsonElement value, string dateTimeFormat = null)
        {            
            TypeCode typeCode = Type.GetTypeCode(property.PropertyType);

            switch (typeCode)
            {
                case TypeCode.Int32:                    
                    int int32Value = value.GetInt32();
                    property.SetValue(obj, int32Value);
                    break;
                case TypeCode.Int64:                    
                    long int64Value = value.GetInt64();
                    property.SetValue(obj, int64Value);
                    break;
                case TypeCode.Double:
                    double doubleValue = value.GetDouble();
                    property.SetValue(obj, doubleValue);
                    break;
                case TypeCode.Decimal:
                    decimal decimalValue = value.GetDecimal();
                    property.SetValue(obj, decimalValue);
                    break;
                case TypeCode.Boolean:
                    bool boolValue = value.GetBoolean();
                    property.SetValue(obj, boolValue);
                    break;
                case TypeCode.DateTime:
                    DateTime dateTimeValue;
                    if(value.ValueKind == JsonValueKind.Null)
                    {
                        dateTimeValue = DateTime.MinValue;
                    } 
                    else if(!string.IsNullOrEmpty(dateTimeFormat))
                    {
                        var stringDateTimeValue = value.GetString();
                        dateTimeValue = DateTime.ParseExact(stringDateTimeValue, dateTimeFormat, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        dateTimeValue = value.GetDateTime();
                    }                    
                    
                    property.SetValue(obj, dateTimeValue);
                    break;
                case TypeCode.String:
                    string stringValue = value.GetString();
                    property.SetValue(obj, stringValue);
                    break;
                default:
                    //ThrowNotImplementedException(type); TODO: Сделать исключение.
                    break;
            }
        }

        public static void SetSharePointUrlFieldFromJson(this PropertyInfo property, Object obj, JsonElement value)
        {
            var spUrlField = new SharePointUrlField
            {
                Url = value.GetProperty("Url").GetString(),
                Description = value.GetProperty("Description").GetString()
            };
            property.SetValue(obj, spUrlField);
        }
    }
}
