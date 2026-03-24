using Infrastructure.Ads;
using Infrastructure.Utilities;
using Newtonsoft.Json;
using System;

namespace Features.PlayerProfile
{
    public static class PlayerProfileAnalytics
    {
        private const string PlayerProfileScreen = "player_profile";
        private const string NameChanged          = "player_profile_name_changed";
        private const string CosmeticChanged      = "player_profile_cosmetic_changed";

        [Serializable]
        private class CosmeticsDto 
        {
            public string avatarId;
        }

        public static void SendNameChanged(int levelNumber, string context)
        {
            AnalyticSender.SendEvent(NameChanged, levelNumber, string.Empty, context);
        }


        public static void SendCosmeticChanged(int levelNumber, string avatarId, string context)
        {
            var data = new CosmeticsDto() { avatarId = avatarId };
            AnalyticSender.SendEvent(CosmeticChanged, levelNumber, JsonConvert.SerializeObject(data, JsonUtils.SerializerSettings), context);
        }


        public static void SendProfileOpened()
        {
            AnalyticSender.TrackScreen(PlayerProfileScreen);
        }
    }
}