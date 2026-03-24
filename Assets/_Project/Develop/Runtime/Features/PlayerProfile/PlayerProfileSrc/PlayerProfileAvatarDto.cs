using Newtonsoft.Json;
using System;


namespace Features.PlayerProfile
{
    [Serializable]
    public class PlayerProfileAvatarDto
    {
        [JsonProperty("a")] public string avatarId;
        //[JsonProperty("f")] public string frameId;
    }
}