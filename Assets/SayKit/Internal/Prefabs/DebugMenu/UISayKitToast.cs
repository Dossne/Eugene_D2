using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable CompareOfFloatsByEqualityOperator

#endregion

namespace SayKitInternal
{
    public class UISayKitToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup uiCanvasGroup;
        [SerializeField] private VerticalLayoutGroup uiContentVerticalLayoutGroup;
        [SerializeField] private Image uiImage;
        [SerializeField] private Text uiText;
        [Range(.1f, .8f)] [SerializeField] private float fadeDuration = .3f;

        private const int MaxTextLength = 300;

        private void Awake()
        {
            uiCanvasGroup.alpha = 0f;
        }
        
        public void Show(string text, float duration, Color color, ToastPosition position)
        {
            uiText.text = (text.Length > MaxTextLength) ? text.Substring(0, MaxTextLength) + "..." : text;
            uiImage.color = color;

            uiContentVerticalLayoutGroup.childAlignment = (TextAnchor)((int)position);

            Dismiss();
            StartCoroutine(FadeInOut(duration, fadeDuration));
        }

        private IEnumerator FadeInOut(float toastDuration, float fadeDuration)
        {
            yield return null;
            uiContentVerticalLayoutGroup.CalculateLayoutInputHorizontal();
            uiContentVerticalLayoutGroup.CalculateLayoutInputVertical();
            uiContentVerticalLayoutGroup.SetLayoutHorizontal();
            uiContentVerticalLayoutGroup.SetLayoutVertical();
            yield return null;
            yield return Fade(uiCanvasGroup, 0f, 1f, fadeDuration);
            yield return new WaitForSeconds(toastDuration);
            yield return Fade(uiCanvasGroup, 1f, 0f, fadeDuration);
        }

        private IEnumerator Fade(CanvasGroup cGroup, float startAlpha, float endAlpha, float fadeDuration)
        {
            var startTime = Time.time;
            var alpha = startAlpha;

            if (fadeDuration > 0f)
            {
                while (alpha != endAlpha)
                {
                    alpha = Mathf.Lerp(startAlpha, endAlpha, (Time.time - startTime) / fadeDuration);
                    cGroup.alpha = alpha;

                    yield return null;
                }
            }

            cGroup.alpha = endAlpha;
        }

        public void Dismiss()
        {
            StopAllCoroutines();
            uiCanvasGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            SayKitToast.IsLoaded = false;
        }
    }
}