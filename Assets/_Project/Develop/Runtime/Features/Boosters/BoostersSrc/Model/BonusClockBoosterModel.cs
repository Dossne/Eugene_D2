using Infrastructure.Localization;

namespace Features.Boosters.Model
{
    public sealed class BonusClockBoosterModel : BoosterModel
    {
        public BonusClockBoosterModel(BoosterData configData, BoosterItemState saveState) : base(configData, saveState)
        {
        }
        
        public override string Description => LocalizationService.I.Get(configData.descriptionKey, configData.boostedValue.ToString());
    }
}