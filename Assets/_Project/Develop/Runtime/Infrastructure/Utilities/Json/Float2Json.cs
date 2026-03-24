using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Optimized format: 2 decimal places (for 1.123456 = 1.12), trailing zeros are discarded (for 1.100000 = 1.1; for 1.00000 = 1).
    /// </summary>
    public class Float2Json : JsonConverter<float>
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
        {
            writer.WriteRawValue(value.ToString("0.##", Invariant));
        }

        public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Convert.ToSingle(reader.Value, Invariant);
        }
    }
}