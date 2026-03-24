using Newtonsoft.Json;
using System;


namespace Features.PlayerProfile
{
    [Serializable]
    public class PlayerProfileData
    {
        [JsonProperty("n")]  public string displayName       = string.Empty;
        [JsonProperty("a")]  public string avatarId          = string.Empty;
        [JsonProperty("tc")] public bool   tutorialCompleted = false;
    }
}