using UnityEngine;

namespace Features.Widgets
{
    public interface IWidgetReadable
    {
        public RectTransform RectTransform { get; }
        public Vector3 IconPosition { get; }
        public Vector3 ScoringTargetOffset { get; }
        public WidgetId Id { get; }
    }
}