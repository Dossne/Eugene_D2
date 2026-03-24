using System.Globalization;
using Infrastructure.Localization;

namespace Features.Boosters.Model
{
    public sealed class InGameBoosterModelBase : BoosterModel
    {
        public InGameBoosterModelBase(BoosterData configData, BoosterItemState saveState) : base(configData, saveState)
        {
        }


        public override string Description => LocalizationService.I.Get(configData.descriptionKey, configData.durationSec.ToString(CultureInfo.InvariantCulture));
    }
}