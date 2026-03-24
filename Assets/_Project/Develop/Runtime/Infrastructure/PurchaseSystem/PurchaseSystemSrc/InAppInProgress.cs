using System;

namespace Infrastructure.PurchaseSystem
{
    [Serializable]
    public class InAppInProgress
    {
        public enum ProgressState
        {
            initiated,
            completed,
        }


        public OfferId inAppOfferId;
        public ProgressState progressState;
        public OpeningMethod openingMethod;
        public string source;

        public InAppInProgress(OfferId inAppOfferId, OpeningMethod openingMethod, string source)
        {
            this.inAppOfferId = inAppOfferId;
            this.progressState = ProgressState.initiated;
            this.openingMethod = openingMethod;
            this.source = source;
        }
    }
}