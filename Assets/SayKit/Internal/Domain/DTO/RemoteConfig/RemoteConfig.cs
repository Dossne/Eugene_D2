using System;
using System.Linq;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

#endregion

namespace SayKitInternal
{
    [UnityEngine.Scripting.Preserve]
    public class RemoteConfig
    {
        [JsonConstructor]
        public RemoteConfig() { }
        
        private const string SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES = "SAYKIT_UNITY_OVERRIDE_SYSTEM_LANGUAGES";

        [JsonProperty("ads_places")]
        public RCAdsPlaceDTO[] ads_places { get; set; }
        
        [JsonProperty("ads_groups")]
        public RCAdsGroupDTO[] ads_groups { get; set; }

        [JsonProperty("ads_settings")]
        public RCAdsSettingsDTO ads_settings { get; set; } = new RCAdsSettingsDTO();

        [JsonProperty("runtime")]
        public RCRuntimeDTO runtime { get; set; } = new RCRuntimeDTO();

        [JsonProperty("game_settings")]
        public SayKitGameConfig game_settings { get; set; } = new SayKitGameConfig();
        
        public RCAdsPlaceDTO findAdsPlace(string place)
        {
            return Array.Find(ads_places, item => item.place == place);
        }
        public RCAdsGroupDTO findAdsGroup(string group)
        {
            return Array.Find(ads_groups, item => item.group == group);
        }
        
        public RemoteConfig DeepCopy()
        {
            return new RemoteConfig
            {
                ads_settings = SKUtils.DeepCopy(ads_settings),
                runtime = SKUtils.DeepCopy(runtime),
                game_settings = SKUtils.DeepCopy(game_settings),
                ads_places = (ads_places ?? Array.Empty<RCAdsPlaceDTO>())
                    .Select(place => place.Clone())
                    .ToArray(),
                ads_groups = (ads_groups ?? Array.Empty<RCAdsGroupDTO>())
                    .Select(group => group.Clone())
                    .ToArray(),
            };
        }
    }
}