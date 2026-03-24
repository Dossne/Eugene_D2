#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

using Newtonsoft.Json;

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class LiveRequestPayerPrediction
    {
        [JsonConstructor]
        public LiveRequestPayerPrediction() { }
        
        [JsonProperty("result")] 
        public bool Result { get; set; }
        
        [JsonProperty("data")] 
        public LiveRequestPayerPredictionData Data { get; set; }
        
        [JsonProperty("error")] 
        public string Error { get; set; }
    }
    
    [UnityEngine.Scripting.Preserve]
    public class LiveRequestPayerPredictionData
    {
        [JsonConstructor]
        public LiveRequestPayerPredictionData() { }
        
        [JsonProperty("probability")] 
        public float Probability { get; set; }
    }
}