using Features.Collectables;
using Infrastructure.Localization;
using Infrastructure.SpriteAtlasControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.TooltipControl
{
    public class EatTutorialTooltip : CustomTooltip
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image icon;
        private CollectablesData collectablesData;
        private SpriteAtlasService spriteAtlasService;

        public void Construct(CollectablesData collectablesData, SpriteAtlasService spriteAtlasService) 
        { 
            this.collectablesData = collectablesData;
            this.spriteAtlasService = spriteAtlasService;
        }

        protected override void Setup()
        {
            text.text = LocalizationService.I.Get(LocKeys.Tooltips.EatTutorialTooltip);
            icon.sprite = spriteAtlasService.GetFromMain(collectablesData.iconName);
        }
    }
}