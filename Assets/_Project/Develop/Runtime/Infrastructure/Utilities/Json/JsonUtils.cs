using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Infrastructure.Utilities
{
    public static class JsonUtils
    {
        private static StringBuilder sb = new();
        
        public static JsonSerializerSettings SerializerSettings => new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
        };

        public static string CreateValue(string key, string value)
        {
            return CreateValue(key, value, isAddQuotesToValue: true);
        }

        public static string CreateValue(string key, StringBuilder value)
        {
            return CreateValue(key, value.ToString(), isAddQuotesToValue: false);
        }

        public static string CreateValue(string key, int value)
        {
            return CreateValue(key, value.ToString(), isAddQuotesToValue: false);
        }

        public static string CreateValue(string key, float value)
        {
            return CreateValue(key, value.ToString("F2", CultureInfo.InvariantCulture), isAddQuotesToValue: false);
        }

        public static string CreateValue(string key, double value)
        {
            return CreateValue(key, value.ToString("F2", CultureInfo.InvariantCulture), isAddQuotesToValue: false);
        }

        public static string CreateValue<T>(string key, List<T> values)
        {
            string resultValue = string.Empty;
            foreach (T value in values)
            {
                resultValue += value.ToString() + ",";
            }

            if (resultValue == "")
            {
                return CreateValue(key, resultValue, isAddQuotesToValue: true);
            }
            resultValue = resultValue.Remove(resultValue.Length - 1);
            return CreateValue(key, resultValue, isAddQuotesToValue: true);
        }
        
        public static void AppendLast(this StringBuilder sb, char text, char removedSeparator = ',')
        {
            if(sb[^1] == removedSeparator)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append(text);
        }

        public static string CreateValue(string key, string value, bool isAddQuotesToValue)
        {
            sb.Clear();
            sb.Append('"');
            sb.Append(key);
            sb.Append('"');
            sb.Append(':');

            AppendQuotes(ref sb, isAddQuotesToValue);
            sb.Append(value);
            AppendQuotes(ref sb, isAddQuotesToValue);

            sb.Append(",");
            return sb.ToString();
        }

        private static void AppendQuotes(ref StringBuilder sb, bool isStringValue)
        {
            if (!isStringValue)
            {
                return;
            }

            sb.Append('"');
        }
    }
}
