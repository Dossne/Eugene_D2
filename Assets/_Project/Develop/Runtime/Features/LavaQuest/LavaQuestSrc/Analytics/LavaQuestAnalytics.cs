using Infrastructure.Ads;

namespace Features.LavaQuest
{
    public static class LavaQuestAnalytics
    {
        private const string Available = "lava_quest_available";
        private const string Opened = "lava_quest_opened";
        private const string LavaQuest = "lava_quest";
        
        public static void SendOnChangeState(LQStateType state, int levelNumber, int eventReadyStartTimes)
        {
            switch (state)
            {
                case LQStateType.ReadyStart:
                    SendAvailable(levelNumber, eventReadyStartTimes);
                    break;
                case LQStateType.Started:
                    SendEventPopupOpened(levelNumber, eventReadyStartTimes);
                    break;
            }
        }


        public static void SendLevelExtraStarted(int levelNumber, int lavaQuestStepNumber)
        {
            AnalyticSender.TrackLevelExtraStarted(levelNumber.ToString(), lavaQuestStepNumber, LavaQuest, GetContext());
            //UnityEngine.Debug.Log($"SendLevelExtraStarted. levelNumber {levelNumber}. lavaQuestStepNumber: {lavaQuestStepNumber}");
        }


        public static void SendLevelExtraCompleted(int levelNumber, int lavaQuestStepNumber)
        {
            AnalyticSender.TrackLevelExtraCompleted(levelNumber.ToString(), lavaQuestStepNumber, 0, LavaQuest, GetContext());
            //UnityEngine.Debug.Log($"TrackLevelExtraCompleted. levelNumber {levelNumber}. lavaQuestStepNumber: {lavaQuestStepNumber}");
        }


        public static void SendLevelExtraFailed(int levelNumber, int lavaQuestStepNumber)
        {
            AnalyticSender.TrackLevelExtraFailed(levelNumber.ToString(), lavaQuestStepNumber, 0, LavaQuest, GetContext());
            //UnityEngine.Debug.Log($"SendLevelExtraFailed. levelNumber {levelNumber}. lavaQuestStepNumber: {lavaQuestStepNumber}");
        }


        private static void SendAvailable(int levelNumber, int eventReadyStartTimes)
        {
            AnalyticSender.TrackTagEvent(Available, levelNumber.ToString(), eventReadyStartTimes, 0, null, GetContext());
            //UnityEngine.Debug.Log($"Track Available. levelNumber: {levelNumber}. eventReadyStartTimes: {eventReadyStartTimes}");
        }


        private static void SendEventPopupOpened(int levelNumber, int eventReadyStartTimes)
        {
            AnalyticSender.TrackTagEvent(Opened, levelNumber.ToString(), eventReadyStartTimes, 0, null, GetContext());
            //UnityEngine.Debug.Log($"Track Opened. levelNumber: {levelNumber}. eventReadyStartTimes: {eventReadyStartTimes}");
        }


        private static string GetContext()
        {
            return null;
        }
    }
}