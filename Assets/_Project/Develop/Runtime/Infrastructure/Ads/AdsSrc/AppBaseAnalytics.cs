namespace Infrastructure.Ads
{
    /// <summary>
    /// Main scene scope
    /// </summary>
    public class AppBaseAnalytics
    {
        private bool isTracked;


        public void TrackAppLoaded()
        {
            if (isTracked)
                return;

            AnalyticSender.TrackAppLoaded();

            isTracked = true;
        }
    }
}