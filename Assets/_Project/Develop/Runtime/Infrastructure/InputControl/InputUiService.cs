using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Infrastructure.InputControl
{
    public class InputUiService
    {
        private InputActionMap uiInputActionMap;
        private bool isInit = false;


        public void Initialize()
        {
            if (isInit)
            {
                return;
            }
            uiInputActionMap = (EventSystem.current.currentInputModule as InputSystemUIInputModule).actionsAsset.FindActionMap("UI");
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
            {
                return;
            }
            uiInputActionMap = null;
            isInit = false;
        }


        public void EnableInput()
        {
            if(!uiInputActionMap.enabled)
            {
                uiInputActionMap.Enable();
            }
        }


        public void DisableInput()
        {
            if (uiInputActionMap.enabled)
            {
                uiInputActionMap.Disable();
            }
        }
    }
}