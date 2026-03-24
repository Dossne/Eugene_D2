#if UNITY_EDITOR

using Newtonsoft.Json;
using Infrastructure.Reward;
using UnityEngine;

namespace Infrastructure.JsonCreator
{
    public abstract class JsonCreator_Editor<T> : ScriptableObject, IJsonCreator where T : class
    {
        [SerializeField] protected T targetObject;

        [SerializeField][TextArea(10, 40)] public string generatedJson;

        void IJsonCreator.ToJson()
        {
            generatedJson = GetJson();
        }

        void IJsonCreator.FromJson()
        {
            targetObject = RewardUtils.ConvertFromJson<T>(generatedJson);;
        }

        private string GetJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            string json = JsonConvert.SerializeObject(targetObject, settings);

            return json;
        }
    }
}

#endif