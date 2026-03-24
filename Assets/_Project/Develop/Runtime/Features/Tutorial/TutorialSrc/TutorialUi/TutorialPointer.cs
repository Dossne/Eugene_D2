using Infrastructure.BroTweens;
using Infrastructure.Popups;
using UnityEngine;


namespace Features.Tutorial
{
    public class TutorialPointer : MonoBehaviour
    {
        [SerializeField] private OpenCloseAnimator openCloseAnimator;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private float curveDurationSec = 1;
        [SerializeField] private RectTransform pointerIconTransform;
        [SerializeField] private RectTransform fromAnchor;
        [SerializeField] private RectTransform toAnchor;

        private BroTweenSafe animatedViewTween;

        private bool isInit;

        public void Initialize()
        {
            if (isInit)
                return;

            openCloseAnimator.Initialize();

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            openCloseAnimator.Close();

            isInit = true;
        }

        public void Deinitialize()
        {
            if (!isInit)
                return;

            animatedViewTween.Kill();
            openCloseAnimator.Deinitialize();

            isInit = false;
        }

        public void Show()
        {
            openCloseAnimator.Open();
            openCloseAnimator.OnOpenComplete += OpenCloseAnimator_OnOpenComplete;
        }

        public void Close()
        {
            animatedViewTween.Kill();
            openCloseAnimator.Close();
        }

        public void Dispose()
        {
            animatedViewTween.Kill();
            Deinitialize();
            Destroy(gameObject);
        }

        private void OpenCloseAnimator_OnOpenComplete()
        {
            animatedViewTween.Kill();
            BroSequence seq = BroTween.SequenceLoop(-1, LoopType.Yoyo)
                                      .SetUpdate(true)
                                      .Insert(0, BroTween.Position(pointerIconTransform,
                                                                   fromAnchor.position,
                                                                   toAnchor.position,
                                                                   curveDurationSec).SetEase(curve));

            animatedViewTween = seq.ToSafe();
            animatedViewTween.Play();
            openCloseAnimator.OnOpenComplete -= OpenCloseAnimator_OnOpenComplete;
        }
    }
}