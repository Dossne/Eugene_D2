using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class FpsTrackerData
    {
        [JsonConstructor]
        public FpsTrackerData(int[] fps)
        {
            Fps = fps;
        }
        
        [JsonProperty("fps")]
        public int[] Fps { get; set; }
    }
}