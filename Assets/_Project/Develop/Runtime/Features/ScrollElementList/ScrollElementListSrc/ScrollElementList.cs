using System;
using System.Collections.Generic;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Features.ScrollList
{
    public abstract class ScrollElementList<T> : MonoBehaviour
    {
        public Action<T, RectTransform> OnElementClicked;
        public Action<T> OnElementShown;

        [Header("Main")]
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected LayoutGroup contentLayout;
        [SerializeField] protected Transform elementsRoot;
        [SerializeField] protected RectTransform viewportRect;
        [SerializeField] protected List<ScrollElement<T>> elements = new();
        [SerializeField] private float paddingTop;
        [SerializeField] private float paddingBottom;
        [SerializeField] private float verticalSpacing;

        [Header("Animations")]
        [SerializeField] private bool isAnimationCurve;
        [SerializeField, HideIf("isAnimationCurve")] private Ease ease = Ease.InOutExpo;
        [SerializeField, ShowIf("isAnimationCurve")] private AnimationCurve easeCurve;
        [SerializeField] private float scrollDuration = 1f;

        private BroTweenSafe scrollTween;
        public List<ScrollElement<T>> Elements => elements;

        private bool isScrollEnabled = true;
        public float VerticalSpacing => verticalSpacing;
        public Transform ElementsRoot => elementsRoot;

        private void OnDisable()
        {
            scrollTween.Kill();
        }

        public void AddElement(ScrollElement<T> scrollElement, int index = -1)
        {
            scrollElement.transform.SetParent(elementsRoot, false);
            scrollElement.SetViewportRect(viewportRect);
            scrollElement.Initialize();
            scrollElement.OnClicked += ElementClickHandler;
            scrollElement.OnShownInViewport += ElementShowHandler;

            elements.Add(scrollElement);
            if (index != -1)
                scrollElement.transform.SetSiblingIndex(index);
        }

        public void RemoveElement(ScrollElement<T> scrollElement)
        {
            scrollElement.Deinitialize();
            Destroy(scrollElement.gameObject);
            scrollElement.OnClicked -= ElementClickHandler;
            scrollElement.OnShownInViewport -= ElementShowHandler;
            elements.Remove(scrollElement);
        }

        public void Clear()
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                elements[i].Deinitialize();
                Destroy(elements[i].gameObject);
                elements[i].OnClicked -= ElementClickHandler;
                elements[i].OnShownInViewport -= ElementShowHandler;
            }

            elements.Clear();

            if (elementsRoot.childCount <= 0)
                return;

            for (int i = elementsRoot.childCount - 1; i >= 0; i--)
                Destroy(elementsRoot.GetChild(i).gameObject);
        }

        public void SetScrollEnabled(bool isEnabled)
        {
            if (isScrollEnabled == isEnabled)
                return;

            isScrollEnabled = isEnabled;
            scrollRect.enabled = isScrollEnabled;

            if (!isScrollEnabled)
                return;

            for (int i = 0; i < elements.Count; i++)
                elements[i].EnableViewportCalculation();
        }

        public float GetPosition(int elementIdx, float offset)
        {
            if (IsAnchoredToTop(scrollRect.content))
                return GetVerticalPositionFromTop(elementIdx, offset);

            if (IsAnchoredToBottom(scrollRect.content))
                return GetVerticalPositionFromBottom(elementIdx, offset);

            return 0f;
        }

        public void ScrollToElement(int index, bool isAnimated = false)
        {
            //TODO : fix for using different layouts/alignments

            if (elements.Count <= 0)
                return;

            float afterAnimationFix = -1; //prevents move up on second+ opening

            var offsetY = contentLayout == null ? 0f : contentLayout.padding.top + afterAnimationFix;
            var offsetX = contentLayout == null ? 0f : contentLayout.padding.left + afterAnimationFix;

            if (elements.Count <= index || index < 0)
                index = 0;

            var target = elements[index].GetComponent<RectTransform>();

            Vector2 newPosition = scrollRect.transform.InverseTransformPoint(target.position);
            newPosition = new Vector2(newPosition.x - offsetX, newPosition.y + offsetY);

            Vector2 contentPosition = scrollRect.transform.InverseTransformPoint(scrollRect.content.position);

            if (!scrollRect.horizontal)
                newPosition.x = contentPosition.x;

            if (!scrollRect.vertical)
                newPosition.y = contentPosition.y;

            Vector2 to = contentPosition - newPosition;

            Scroll(isAnimated, to);
        }

        public void Scroll(bool isAnimated, Vector2 to)
        {
            if (isAnimated)
            {
                AnimatedScrollTo(to);
            }
            else
            {
                InstantScrollTo(to);
            }
        }

        public Vector2 GetContentRootPosition()
        {
            return scrollRect.content.anchoredPosition;
        }

        public void SetContentRootPosition(Vector2 to)
        {
            scrollRect.content.anchoredPosition = to;
        }

        /// <summary>
        /// Top to bottom order
        /// </summary>
        [Button]
        public void RefreshVerticalLayout()
        {
            if (Elements.Count == 0)
                return;

            scrollRect.content.StretchHorizontalTop();
            float currentY = -paddingTop;
            float totalHeight = paddingTop;

            for (int i = 0; i < Elements.Count; i++)
            {
                var elem = Elements[i];
                
                if(!elem.gameObject.activeSelf)
                    continue;
                
                elem.ElementRect.StretchHorizontalTop();

                var pos = new Vector2(0f, currentY);
                elem.SetAnchoredPosition(pos);

                currentY -= elem.RectHeight;
                totalHeight += elem.RectHeight;

                if (i < Elements.Count - 1)
                {
                    currentY -= verticalSpacing;
                    totalHeight += verticalSpacing;
                }
            }

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalHeight + paddingBottom);
        }

        /// <summary>
        /// Bottom to top order
        /// </summary>
        [Button]
        public void RefreshVerticalLayoutReversed()
        {
            if (Elements.Count == 0)
                return;

            scrollRect.content.StretchHorizontalBottom();
            float currentY = paddingBottom;
            float totalHeight = paddingBottom;

            for (int i = 0; i < Elements.Count; i++)
            {
                var elem = Elements[i];
                if(!elem.gameObject.activeSelf)
                    continue;
                
                elem.ElementRect.StretchHorizontalBottom();

                var pos = new Vector2(0f, currentY);
                elem.SetAnchoredPosition(pos);

                currentY += elem.RectHeight;
                totalHeight += elem.RectHeight;

                if (i < Elements.Count - 1)
                {
                    currentY += verticalSpacing;
                    totalHeight += verticalSpacing;
                }
            }

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalHeight + paddingTop);
        }

        protected BroTweenBase GetScrollTween(Vector2 to, float duration, bool isAnimationCurve, Ease ease, AnimationCurve easeCurve)
        {
            var tween = BroTween.AnchoredPosition(scrollRect.content, to, duration).SetUpdate(true);

            if (isAnimationCurve || ease == Ease.Custom)
                tween.SetEase(easeCurve);
            else
                tween.SetEase(ease);

            return tween;
        }

        private void AnimatedScrollTo(Vector2 to)
        {
            SetScrollEnabled(false);
            scrollTween = GetScrollTween(to, scrollDuration, isAnimationCurve, ease, easeCurve).OnComplete(() => SetScrollEnabled(true)).ToSafe();
            scrollTween.Play();
        }

        private void InstantScrollTo(Vector2 to)
        {
            SetScrollEnabled(false);
            SetContentRootPosition(to);
            SetScrollEnabled(true);
        }

        /// <summary>
        /// Calculate when contents scrolls when 0 element is at top (order in ElementsList is direct: 0 to n) 
        /// </summary>
        private float GetVerticalPositionFromTop(int elementIdx, float offset)
        {
            if (Elements.Count == 0 || elementIdx <= 0)
                return 0;

            var viewportHeight = scrollRect.viewport.rect.height;
            var contentHeight = scrollRect.content.rect.height;
            var scrollMaxPos = Mathf.Max(0, contentHeight - viewportHeight);
            var height = GetTotalHeightToIdx(elementIdx, verticalSpacing, paddingTop);
            height -= offset;
            return Mathf.Clamp(height, 0, scrollMaxPos);
        }

        /// <summary>
        /// Calculate when contents scrolls when 0 element is at bottom (order in ElementsList is direct: 0 to n) 
        /// </summary>
        private float GetVerticalPositionFromBottom(int elementIdx, float offset)
        {
            if (Elements.Count == 0 || elementIdx <= 0)
                return 0;

            var viewportHeight = scrollRect.viewport.rect.height;
            var contentHeight = scrollRect.content.rect.height;
            var scrollMinPos = viewportHeight - contentHeight;
            var height = GetTotalHeightToIdx(elementIdx, verticalSpacing, paddingBottom);
            height -= offset;
            height = -height;
            return Mathf.Clamp(height, scrollMinPos, 0f);
        }

        private float GetTotalHeightToIdx(int elementIdx, float spacingY, float padding)
        {
            float result = padding;

            for (int i = 0; i < elementIdx; i++)
            {
                if(!Elements[i].gameObject.activeSelf)
                    continue;
                
                result += Elements[i].RectHeight;

                if (i > 0)
                    result += spacingY;
            }

            return result;
        }

        private bool IsAnchoredToTop(RectTransform rt)
        {
            return Mathf.Approximately(rt.anchorMin.y, 1f) && Mathf.Approximately(rt.anchorMax.y, 1f);
        }

        private bool IsAnchoredToBottom(RectTransform rt)
        {
            return Mathf.Approximately(rt.anchorMin.y, 0f) && Mathf.Approximately(rt.anchorMax.y, 0f);
        }

        private void ElementClickHandler(T elementDataSource, RectTransform rectTransform)
        {
            OnElementClicked?.Invoke(elementDataSource, rectTransform);
        }

        private void ElementShowHandler(T elementDataSource)
        {
            OnElementShown?.Invoke(elementDataSource);
        }
    }
}