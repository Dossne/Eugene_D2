using UnityEngine;

namespace Features.TargetMarker
{
    public class TargetMarkerEntry
    {
        private enum State
        {
            None,
            Pointer,
            Marker
        }

        public PoolableTargetMarker markerView;
        public PoolableTargetIconPointer pointerView;
        public Transform target;

        private State state;
        public bool NeedRemove { get; private set; }


        public void MarkForRemove()
        {
            NeedRemove = true;
        }


        public void ReleaseViews()
        {
            pointerView.ReleaseToPool();
            markerView.ReleaseToPool();
        }


        public void SwitchToPointer()
        {
            if (state == State.Pointer)
                return;

            markerView.SetObjectActive(false);

            pointerView.SetObjectActive(true);
            pointerView.PlayScaleAnimation();

            state = State.Pointer;
        }


        public void SwitchToMarker()
        {
            if (state == State.Marker)
                return;

            pointerView.SetObjectActive(false);

            markerView.SetObjectActive(true);
            markerView.PlayScaleAnimation();

            state = State.Marker;

        }
    }
}