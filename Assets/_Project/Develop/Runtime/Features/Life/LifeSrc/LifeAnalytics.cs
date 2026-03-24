using Infrastructure.Ads;

namespace Features.Life
{
    public class LifeAnalytics
    {
        const string HEALTH_INCOME_TAG = "health_income";
        const string HEALTH_OUTCOME_TAG = "health_outcome";

        public static void SendIncomeEvent(int amount, int totalAmount, string reason) 
        {
            AnalyticSender.SendEvent(HEALTH_INCOME_TAG, amount, totalAmount, reason);
        }

        public static void SendOutcomeEvent(int amount, int totalAmount, string reason)
        {
            AnalyticSender.SendEvent(HEALTH_OUTCOME_TAG, amount, totalAmount, reason);
        }
    }
}