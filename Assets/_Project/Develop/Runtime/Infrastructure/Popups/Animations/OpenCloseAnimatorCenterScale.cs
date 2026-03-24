using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using UnityEngine;

namespace Infrastructure.Popups
{
    public class OpenCloseAnimatorCenterScale : OpenCloseAnimator
    {
        private const float DefaultValue = 1f;
        private const float ZeroValue = 0f;

        [Header("Main components")]
        [SerializeField] private CanvasGroup bGCanvasGroup;
        [SerializeField] private RectTransform animatingRoot;
        [SerializeField] private CanvasGroup animatingRootCg;

        [Header("Settings")]
        [SerializeField] private OpenCloseAnimatorCenterScaleSettings settings;

        [Header("Debug")]
        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe openSeq;
        [SerializeField, TriInspector.ReadOnly] private BroTweenSafe closeSeq;
        private GameObject rootGameObject;
        private string goName = "root_go_name_not_set";


        public override void Initialize()
        {
            rootGameObject = gameObject;
            goName = gameObject.name;
        }


        public override void Deinitialize()
        {
            openSeq.Kill();
            closeSeq.Kill();
            
            OnOpenComplete = null;
            OnCloseComplete = null;
        }


        public override void ActivateAsInvisible()
        {
            bGCanvasGroup.alpha = 0;
            animatingRootCg.alpha = 0;
            animatingRoot.localScale = Vector3.one * DefaultValue;
            SetObjectEnabled();
        }


        public override void Open()
        {
            SetObjectEnabled();

            bGCanvasGroup.alpha = settings.alphaMin;
            animatingRoot.localScale = Vector3.one * ZeroValue;

            closeSeq.Stop();

            if (!openSeq.TryRewind())
                CreateOpenSequence();
            
            openSeq.Play();
        }


        public override void Close()
        {
            if (bGCanvasGroup != null)
                bGCanvasGroup.alpha = DefaultValue;

            if (animatingRoot != null)
                animatingRoot.localScale = Vector3.one;

            openSeq.Stop();

            if (!closeSeq.TryRewind())
                CreateCloseSequence();

            closeSeq.Play();
        }

        public override void InstantClose()
        {
            SetObjectDisabled();
            ForceStop();
        }

        public override void ForceStop()
        {
            openSeq.Stop();
            closeSeq.Stop();
        }


        public override UniTask OpenAsync(CancellationToken token)
        {
            Open();
            return openSeq.ToUniTask(BroTweenCancelBehaviour.CompleteWithCallback, token);
        }


        public override UniTask CloseAsync(CancellationToken token)
        {
            Close();
            return closeSeq.ToUniTask(BroTweenCancelBehaviour.CompleteWithCallback, token);
        }
        
        
        private void CreateOpenSequence()
        {
            var seq = BroTween.Sequence().SetAutoKill(false).SetUpdate(true);

            if (bGCanvasGroup != null)
                seq.Insert(0, BroTween.FadeCanvasGroup(bGCanvasGroup, DefaultValue, settings.bgFadeDuration));

            if (animatingRootCg != null)
                seq.Insert(0, BroTween.FadeCanvasGroup(animatingRootCg, DefaultValue, settings.openDuration));

            if (animatingRoot != null)
                seq.Insert(0, BroTween.Scale(animatingRoot, Vector3.one * DefaultValue, settings.openDuration).SetEase(settings.openScaleCurve));

            seq.OnComplete(() =>
            {
                OnOpenComplete?.Invoke();
            });

            openSeq = seq.ToSafe();
        }


        private void CreateCloseSequence()
        {
            var seq = BroTween.Sequence().SetAutoKill(false).SetUpdate(true);

            if (bGCanvasGroup != null)
                seq.Insert(0, BroTween.FadeCanvasGroup(bGCanvasGroup, settings.alphaMin, settings.bgFadeOutDuration));
            
            if (animatingRootCg != null)
                seq.Insert(0, BroTween.FadeCanvasGroup(animatingRootCg, ZeroValue, settings.closeDuration));
            
            if (animatingRoot != null)
                seq.Insert(0, BroTween.Scale(animatingRoot, Vector3.one * ZeroValue, settings.closeDuration).SetEase(settings.closeScaleCurve));
            
            
            seq.OnComplete(() =>
            {
                SetObjectDisabled();
                OnCloseComplete?.Invoke();
            });

            closeSeq = seq.ToSafe();
        }


        private void SetObjectDisabled()
        {
            if (animatingRoot != null)
                animatingRoot.localScale = Vector3.one * DefaultValue;
            if (rootGameObject != null)
                rootGameObject.SetObjectActive(false);
            else
                Debug.LogWarning($"Root gameobject {goName} is null or destroyed!");
        }


        private void SetObjectEnabled()
        {
            if (rootGameObject != null)
                rootGameObject.SetObjectActive(true);
            else
                Debug.LogWarning($"Root gameobject {goName} is null or destroyed!");
        }

        
#if UNITY_EDITOR
        [TriInspector.Button, TriInspector.ShowInPlayMode]
        private void InitializeAnimations_Editor()
        {
            CreateOpenSequence();
            CreateCloseSequence();
        }
#endif

    }
}