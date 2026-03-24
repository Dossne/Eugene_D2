using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class SayKitDoubleConverter : CustomCreationConverter<double>
    {
        public override double Create(Type objectType)
        {
            return 0.0;
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                return Convert.ToDouble(reader.Value);
            }

            if (reader.TokenType == JsonToken.String)
            {
                var stringValue = (string)reader.Value;
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}