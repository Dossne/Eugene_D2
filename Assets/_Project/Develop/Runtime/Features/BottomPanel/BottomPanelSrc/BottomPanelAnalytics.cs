using Infrastructure.Ads;

namespace Features.BottomPanel
{
    public static class BottomPanelAnalytics
    {
        private const string ShopScreen = "shop_screen";
        private const string MainScreen = "main_screen";
        private const string TeamScreen = "team_screen";

        public static void SendShopOpened()
        {
            AnalyticSender.TrackScreen(ShopScreen);
        }

        public static void SendMainOpened()
        {
            AnalyticSender.TrackScreen(MainScreen);
        }

        public static void SendTeamOpened()
        {
            AnalyticSender.TrackScreen(TeamScreen);
        }
    }
}