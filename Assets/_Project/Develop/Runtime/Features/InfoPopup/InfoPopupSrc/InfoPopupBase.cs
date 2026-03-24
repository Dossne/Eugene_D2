using Infrastructure.Popups;
using UnityEngine;

namespace Features.InfoPopup
{
    public class InfoPopupBase : PopupBase
    {
        [SerializeField] private InfoPopupHandler handler;


        protected override void OnInitialize()
        {
            handler.Initialize();
        }


        protected override void OnDeinitialize()
        {
            handler.Deinitialize();
        }


        protected override void OnBeginOpen()
        {
            handler.Open();
        }


        protected override void OnBeginClose()
        {
            handler.Close();
        }
    }
}