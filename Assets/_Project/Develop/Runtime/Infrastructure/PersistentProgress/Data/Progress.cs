using Features.FreePaidOffer;
using Features.Life;
using Features.RecommendationUi;
using Features.SuperDiscountUi;
using Features.Tutorial;
using Infrastructure.PurchaseSystem;
using System;
using Features.LevelSessionStateControl;
using Newtonsoft.Json;
using Features.SuperSpeedMode;
using Features.PlayerProfile;
using System.Collections.Generic;
using Features.Social;


namespace Infrastructure.PersistentProgress
{
    [Serializable]
    public class Progress
    {
        public AppState appState = new();
        public GameState gameState = new();
        public PurchaseState purchaseState = new();
        public LifeControllerState lifeControllerState = new();
        public RecommendationState recommendationState = new();
        public SuperDiscountState superDiscountState = new();        
        public TutorialState tutorialState = new();
        public FreePaidOfferState freePaidOfferState = new();
        [JsonProperty("lvlSes")] public LevelSessionState levelSessionState = new();
        [JsonProperty("sss")] public SuperSpeedState superSpeedState = new();
        [JsonProperty("ppd")] public PlayerProfileData playerProfileData = new();
        [JsonProperty("lss")] public List<LeaderboardSaveState> leaderboards = new();
#if PR_CHEAT
        public Cheat.CheatState cheatState = new();
#endif

    }
}