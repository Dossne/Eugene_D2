using Features.HudLevelButtons;
using Features.Widgets;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Utilities;
using UnityEngine;

namespace Infrastructure.GameSceneUIControl
{
    public class GameSceneUIService
    {
        private readonly GameObject mainBg;
        private readonly WidgetManager widgetManager;
        private readonly StartLevelButtonView startLevelButtonView;
        private bool isInit;


        public GameSceneUIService(MainUIProvider mainUIProvider,
                                  WidgetManager widgetManager)
        {
            this.mainBg = mainUIProvider.MetaSceneBg;
            this.startLevelButtonView = mainUIProvider.HudProvider.StartLevelButtonView;
            this.widgetManager = widgetManager;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            widgetManager.HideWidgets();
            SetMainBgActive(false);
            startLevelButtonView.SetObjectActive(false);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            isInit = false;
        }


        private void SetMainBgActive(bool value)
        {
            mainBg.SetObjectActive(value);
        }
    }
}