using Newtonsoft.Json;

namespace Infrastructure.Reward
{
    public class RewardUtils
    {
        public static T ConvertFromJson<T>(string json)
        {
            JsonSerializerSettings settings = new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            try
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Error of deserialization JSON: " + ex.Message);
                return default;
            }
        }
    }
}