using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Widgets
{
    public sealed class WidgetRoot : MonoBehaviour
    {
        [SerializeField] private LayoutGroup layoutGroup;
        [SerializeField] private RectTransform rectTransform;

        private readonly List<Widget> children = new();

        private bool isDirty;
        private bool subscribed;

        public RectTransform Root => rectTransform;

        private void OnDisable()
        {
            UnsubscribeFromWillRenderCanvases();
        }

        public void Initialize()
        {
            layoutGroup.enabled = false;
        }

        public void Deinitialize()
        {
            UnsubscribeFromWillRenderCanvases();

            foreach (var item in children)
            {
                if (item != null)
                    item.OnChangeVisibility -= Widget_OnChangeVisibility;
            }

            children.Clear();
        }

        public void Add(Widget item)
        {
            if (item == null)
                return;

            children.Add(item);
            item.OnChangeVisibility += Widget_OnChangeVisibility;
            MarkDirty();
        }

        public bool Remove(Widget item)
        {
            if (item == null)
                return false;

            item.OnChangeVisibility -= Widget_OnChangeVisibility;
            MarkDirty();
            return children.Remove(item);
        }

        private void MarkDirty()
        {
            if (!gameObject.activeInHierarchy)
                return;

            isDirty = true;
            SubscribeOnWillRenderCanvases();
        }

        private void SortAndRebuild()
        {
            if (!gameObject.activeInHierarchy)
                return;

            children.RemoveAll(x => x == null || x.transform.parent != rectTransform);
            children.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            for (int i = 0; i < children.Count; i++)
            {
                children[i].transform.SetSiblingIndex(i);
            }

            layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            layoutGroup.enabled = false;
        }

        private void SubscribeOnWillRenderCanvases()
        {
            if (subscribed) 
                return;
            
            Canvas.willRenderCanvases += Canvas_OnWillRenderCanvases;
            subscribed = true;
        }

        private void UnsubscribeFromWillRenderCanvases()
        {
            if (!subscribed)
                return;

            Canvas.willRenderCanvases -= Canvas_OnWillRenderCanvases;
            subscribed = false;
        }

        private void Widget_OnChangeVisibility()
        {
            MarkDirty();
        }

        private void Canvas_OnWillRenderCanvases()
        {
            if (!isDirty)
            {
                UnsubscribeFromWillRenderCanvases();
                return;
            }

            isDirty = false;

            SortAndRebuild();
        }
    }
}