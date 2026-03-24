using System;
using Infrastructure.BroTweens;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.Character
{
    public class CharacterView : MonoBehaviour
    {

        [Header("Components")]
        [SerializeField] private Transform viewRoot;
        private Transform movementRoot;
        [SerializeField] private LayerChanger layerChanger;
        [SerializeField] private ItemCollectTrigger collectTrigger;
        [SerializeField] private ProgressSceneView progressView;
        [SerializeField] private CharacterFXController fxController;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private GameObject arrow;

        [Header("ChangeScaleConfig")]
        [SerializeField] private AnimationCurve changeScaleEaseCurve;
        [SerializeField] private float changeScaleDuration = 0.3f;
        [SerializeField, Tooltip("Скорость смещения стрелки")] private float arrowInterpolateSpeed = 25f;

        [Header("ChangeArrowSprite")]
        [SerializeField] private SpriteRenderer arrowSpriteRenderer;
        [SerializeField] private Sprite arrowBase;
        [SerializeField] private Sprite arrowSuperSpeed;

        [Header("ChangeCircleSprite")]
        [SerializeField] private SpriteRenderer holeSpriteRenderer;
        [SerializeField] private Sprite circleBase;
        [SerializeField] private Sprite circleBoosted;
        [SerializeField] private Sprite circleSuperSpeed;

        [Header("ChangeTubeMaterial")]
        [SerializeField] private MeshRenderer tubeRenderer;
        [SerializeField] private Material tubeMaterial;
        [SerializeField] private Material superSpeedTubeMaterial;

        [Header("Debug")]
        [SerializeField] private float currentScale;

        private Quaternion startRot;
        private Vector3 startPos;
        private BroTweenSafe scaleTween;
        private Quaternion initialArrowRotationOffset;

        [SerializeField] private PhysicsInterpolationBuffer moveInterpolator;

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetScale();
        }
#endif


        public void Construct(float startScale, Vector3 startPos, Quaternion startRot)
        {
            this.currentScale = startScale;
            this.startPos = startPos;
            this.startRot = startRot;
            initialArrowRotationOffset = arrow.transform.rotation;
            arrow.gameObject.SetActive(false);
            moveInterpolator = new PhysicsInterpolationBuffer();
        }


        public void Initialize()
        {
            SetScale();
            transform.localPosition = Vector3.zero;
            rb.MovePosition(startPos);
            viewRoot.position = startPos;
            transform.localRotation = startRot;

            layerChanger.Initialize();
            progressView.Initialize();
            gameObject.SetActive(true);
            fxController.Initialize();
            SetCharacterViewType(CharacterViewType.Base);
        }


        public void Deinitialize()
        {
            layerChanger.Deinitialize();
            progressView.Deinitialize();
            fxController.Deinitialize();
            moveInterpolator.Clear();
            scaleTween.Kill();
        }


        public void SetScale(float value)
        {
            currentScale = value;
            Vector3 from = viewRoot.localScale;
            Vector3 to = Vector3.one * value;
            
            scaleTween.Kill();
            scaleTween = BroTween.Scale(viewRoot, from, to, changeScaleDuration).SetEase(changeScaleEaseCurve).ToSafe();
            scaleTween.TryPlay();

            rb.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }


        public Vector3 GetPosition()
        {
            return viewRoot.position;
        }


        public void Tick(float inputX, float inputY, in float dt)
        {
            if (moveInterpolator.HaveValues())
            {
                viewRoot.position = moveInterpolator.GetValue();
            }

            Vector3 direction = new Vector3(inputX, 0f, inputY).normalized;

            if (direction.sqrMagnitude >= 0.1f)
            {
                arrow.SetObjectActive(true);

                Quaternion targetArrowRotation = Quaternion.LookRotation(direction) * initialArrowRotationOffset;
                arrow.transform.rotation = Quaternion.Slerp(arrow.transform.rotation, targetArrowRotation, arrowInterpolateSpeed * dt);
            }
        }


        public void FixedTick(float inputX, float inputY, in float moveSpeed, in float dt)
        {
            Vector3 direction = new Vector3(inputX, 0f, inputY).normalized;
            var velocity = direction * moveSpeed;
            rb.MovePosition(rb.position + velocity * dt);
            moveInterpolator.AddValue(rb.position);
        }


        public void SetSizeText(string value)
        {
            progressView.SetText(value);
        }


        public void SetProgressSlider(float progress01, bool smooth)
        {
            progressView.SetProgressSlider(progress01, smooth);
        }


        public void StopMove()
        {
            rb.velocity = Vector3.zero;
            arrow.gameObject.SetObjectActive(false);
        }


        public void PlayLevelUpFx()
        {
            progressView.AnimateText();
            fxController.PlayLevelUpFX();
        }


        public void PlaySizeBoosterFx()
        {
            fxController.PlaySizeBoosterFX();
        }


        public void PlayBoostBottleFx()
        {
            fxController.PlayBoostBottleFx();
        }


        public void PlayDeathVFX(Action callback = null)
        {
            fxController.PlayDeathVFX(callback);
        }

        public void SetActiveSuperSpeedFX(bool isActive)
        {
            fxController.SetActiveSuperSpeedFX(isActive);
        }

        public void SetCharacterViewType(CharacterViewType characterViewType)
        {
            switch (characterViewType)
            {
                case CharacterViewType.Base:
                    holeSpriteRenderer.sprite = circleBase;
                    arrowSpriteRenderer.sprite = arrowBase;
                    tubeRenderer.material = tubeMaterial;
                    break;
                case CharacterViewType.Boosted:
                    holeSpriteRenderer.sprite = circleBoosted;
                    arrowSpriteRenderer.sprite = arrowBase;
                    tubeRenderer.material = tubeMaterial;
                    break;
                case CharacterViewType.SuperSpeed:
                    holeSpriteRenderer.sprite = circleSuperSpeed;
                    arrowSpriteRenderer.sprite = arrowSuperSpeed;
                    tubeRenderer.material = superSpeedTubeMaterial;
                    break;
                default:
                    holeSpriteRenderer.sprite = circleBase;
                    arrowSpriteRenderer.sprite = arrowBase;
                    tubeRenderer.material = tubeMaterial;
                    break;
            }
        }


        public Transform GetViewRoot()
        {
            return viewRoot;
        }


        public Transform GetMovementRoot()
        {
            if (movementRoot == null && viewRoot != null)
            {
                movementRoot = viewRoot.Find("MovementRoot");
            }

            return movementRoot != null ? movementRoot : viewRoot;
        }


        private void SetScale()
        {
            viewRoot.localScale = new Vector3(currentScale, currentScale, currentScale);
            rb.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }
    }
}
