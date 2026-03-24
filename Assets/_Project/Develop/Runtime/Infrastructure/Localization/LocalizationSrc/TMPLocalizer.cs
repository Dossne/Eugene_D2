using System;
using TMPro;

namespace Infrastructure.Localization
{
    [Serializable]
    public class TMPLocalizer
    {
        public TextMeshProUGUI txtItem;
        public string localizeKey;


        public void Localize(string val1 = null, string val2 = null, string val3 = null)
        {
            if (txtItem != null)
            {
                txtItem.SetText(LocalizationService.I.Get(localizeKey, val1, val2, val3));
            }
        }
    }
}