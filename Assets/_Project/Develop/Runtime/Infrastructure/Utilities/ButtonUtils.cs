using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Utilities
{
    public static class ButtonUtils
    {
        public static void SetColorPressed(this Button btn, bool pressed)
        {
            btn.image.color = pressed switch
            {
                true when ColorUtility.TryParseHtmlString(ColorHex.Yellow, out Color yellowColor) => yellowColor,
                false when ColorUtility.TryParseHtmlString(ColorHex.Green, out Color greenColor) => greenColor,
                _ => btn.image.color
            };
        }
    }
}