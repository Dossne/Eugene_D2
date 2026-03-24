using Features.Widgets;
using Infrastructure.MainUICanvasControl;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.HudLevelButtons
{
    public class MetaSceneUIService
    {
        private readonly GameObject mainBg;
        private readonly WidgetManager widgetManager;
        private bool isInit;


        public MetaSceneUIService(MainUIProvider mainUIProvider,
                                  WidgetManager widgetManager)
        {
            this.mainBg = mainUIProvider.MetaSceneBg;
            this.widgetManager = widgetManager;
        }


        public void Initialize()
        {
            if (isInit)
                return;
            
            SetMainBgActive(true);
            widgetManager.ShowWidgets();
            
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            widgetManager.HideWidgets();
            SetMainBgActive(false);

            isInit = false;
        }


        private void SetMainBgActive(bool value)
        {
            mainBg.SetObjectActive(value);
        }
    }
}