using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public enum ToastPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public static class SayKitToast
    {
        public static bool IsLoaded;
        private static UISayKitToast _toastUI;
        public static GameObject Toast;

        private static void Prepare()
        {
            if (IsLoaded)
            {
                return;
            }

            var instance = Object.Instantiate(Toast);
            instance.name = "[SayKitToastUI]";
            _toastUI = instance.GetComponent<UISayKitToast>();
            
            IsLoaded = true;
        }

        public static void Show(string text, float duration, Color color, ToastPosition position)
        {
            Prepare();
            if (_toastUI)
            {
                _toastUI.Show(text, duration, color, position);
            }
        }

        public static void Dismiss()
        {
            if (_toastUI)
            {
                _toastUI.Dismiss();
            }
        }
    }
}