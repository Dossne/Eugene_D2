using Features.Level;
using Features.Social;
using Infrastructure.Utilities;
using System.Text;


namespace Infrastructure.Ads
{
    public class AnalyticsContextCreator 
    {
        private const string ShortContextFieldKey = "short_context_field";
        private const string CurrentLevelNumberKey = "current_level_number";
        private const string PlayerIdKey = "player_id";
        private const string PlayerNameKey = "player_name";

        private readonly LevelService levelService;
        private readonly SocialService socialService;

        private static StringBuilder finalSb = new StringBuilder();



        private string ShortContextFieldJson => JsonUtils.CreateValue(ShortContextFieldKey, "short_context_field");
        private string CurrentLevel => JsonUtils.CreateValue(CurrentLevelNumberKey, levelService.CurrentLevelNumber);
        private string PlayerId => JsonUtils.CreateValue(PlayerIdKey, socialService.GetPlayerId());
        private string PlayerName => JsonUtils.CreateValue(PlayerNameKey, socialService.GetPlayerName());



        public AnalyticsContextCreator(LevelService levelService,
            SocialService socialService)
        {
            this.levelService = levelService;
            this.socialService = socialService;
        }


        public string MainContext
        {
            get
            {
                finalSb.Clear();
                finalSb.Append('{');
                finalSb.Append(PlayerId);
                finalSb.Append(PlayerName);
                finalSb.AppendLast('}');

                return finalSb.ToString();
            }
        }


        public string ShortContext
        {
            get
            {
                finalSb.Clear();
                finalSb.Append('{');
                finalSb.Append(ShortContextFieldJson);
                finalSb.AppendLast('}');
                return finalSb.ToString();
            }
        }
    }
}