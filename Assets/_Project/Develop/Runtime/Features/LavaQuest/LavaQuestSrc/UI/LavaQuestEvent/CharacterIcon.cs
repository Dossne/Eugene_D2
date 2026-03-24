using Infrastructure.BroTweens;
using Infrastructure.Pool.Particles;
using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{
    public class CharacterIcon : MonoBehaviour
    {
        private const string JumpLeft = "JumpLeft";
        private const string JumpRight = "JumpRight";
        private const string Fall = "Fall";

        [SerializeField] private Image backImg;
        [SerializeField] private Image iconImg;
        [SerializeField] private Animator anim;
        [SerializeField] private RectMask2D mask;
        [SerializeField] private ReusableParticleSystem lavaFx;
        [SerializeField] private RectTransform mainRoot;

        public Vector3 CurrentPos => mainRoot.position;
        public Sprite CurrentSprite => iconImg.sprite;
        public Sprite CurrentBack => backImg.sprite;
        public Image IconImage => iconImg;
        public RectTransform MainRoot => mainRoot;

        private bool isInit;

        public void Initialize()
        {
            lavaFx.Initialize();
        }

        public void Dispose()
        {
            lavaFx.Deinitialize();
            Destroy(gameObject);
        }


        public void SetIcon(Sprite icon, Sprite back)
        {
            this.iconImg.sprite = icon;
            this.backImg.sprite = back;
        }


        public void SetIcon(Sprite icon)
        {
            this.iconImg.sprite = icon;
        }


        public void SetPosition(Vector3 worldPos)
        {
            worldPos.y += GetCorrectiveHeight();
            mainRoot.position = worldPos;
        }


        public void ResetToDefaultState()
        {
            lavaFx.Stop();
        }


        public void SetObjectActive(bool value)
        {
            gameObject.SetObjectActive(value);

            if (value)
            {
                ResetState();
            }
        }


        public void SetAlphaZero()
        {
            var imgColor = iconImg.color;
            imgColor.a = 0;
            iconImg.color = imgColor;

            var backColor = backImg.color;
            backColor.a = 0;
            backImg.color = backColor;
        }


        public void PutOnBottomHierarchy()
        {
            mainRoot.SetAsFirstSibling();
        }


        public BroTweenBase GetFadeInIconTween(AnimationCurve curve, float duration)
        {
            return BroTween.FadeImage(iconImg, 0, 1, duration).SetEase(curve);
        }

        public BroTweenBase GetFadeInBackTween(AnimationCurve curve, float duration)
        {
            return BroTween.FadeImage(backImg, 0, 1, duration).SetEase(curve);
        }


        /// <summary>
        /// Create jump sequence (left or right side animation)
        /// </summary>
        /// <param name="curve">AnimationCurve</param>
        /// <param name="duration">in seconds</param>
        /// <param name="from">world position</param>
        /// <param name="to">world position</param>
        /// <returns>Sequence</returns>
        public BroSequence GetJumpSequence(AnimationCurve curve, float duration, Vector3 from, Vector3 to)
        {
            float deltaX = to.x - from.x;
            return GetJumpSequence(curve, duration, from, to, deltaX > 0 ? JumpRight : JumpLeft);
        }


        /// <summary>
        /// Create jump fall sequence (fall in lava)
        /// </summary>
        /// <param name="curve">AnimationCurve</param>
        /// <param name="duration">in seconds</param>
        /// <param name="from">world position</param>
        /// <param name="to">world position</param>
        /// <returns>Sequence</returns>
        public BroSequence GetJumpFallSequence(AnimationCurve curve, float duration, Vector3 from, Vector3 to)
        {
            return GetJumpSequence(curve, duration, from, to, Fall);
        }


        public BroTweenBase GetLocalRotateByZTween(AnimationCurve curve, float duration, float rotationZ)
        {
            return BroTween.RotationLocal(backImg.transform, Vector3.zero, new Vector3(0f, 0f, rotationZ), duration).SetEase(curve);
        }


        /// <summary>
        /// Create move down sequence
        /// </summary>
        /// <param name="curve">AnimationCurve</param>
        /// <param name="duration">in seconds</param>
        /// <param name="from">world position</param>
        /// <param name="to">world position</param>
        /// <returns>Sequence</returns>
        public BroSequence GetMoveDownSequence(AnimationCurve curve, float duration, Vector3 from, Vector3 to)
        {
            from.y += GetCorrectiveHeight();
            to.y += GetCorrectiveHeight();

            BroSequence seq = BroTween.Sequence();
            seq.AppendCallback(() =>
            {
                if (gameObject.activeInHierarchy)
                {
                    lavaFx.Play();
                    mask.enabled = true;
                }
            });
            seq.Insert(0, BroTween.Position(backImg.transform, from, to, duration).SetEase(curve));
            return seq;
        }


        private BroSequence GetJumpSequence(AnimationCurve curve, float duration, Vector3 from, Vector3 to, string animationName)
        {
            BroSequence sequence = BroTween.Sequence();

            sequence.AppendCallback(() =>
            {
                if (gameObject.activeInHierarchy)
                {
                    anim.enabled = true;
                    anim.Play(animationName, 0,0);
                }

            });

            to.y += GetCorrectiveHeight();

            sequence.Insert(0, BroTween.Position(mainRoot, from, to, duration).SetEase(curve));
            return sequence;
        }


        private float GetCorrectiveHeight()
        {
            return mainRoot.rect.height * mainRoot.lossyScale.y * 0.5f;
        }


        private void ResetState()
        {
            mask.enabled = false;
            anim.enabled = false;

            backImg.transform.rotation = Quaternion.identity;
            backImg.transform.localPosition = Vector3.zero;
            
            //reset size delta affected by animator's Jump/Fall animations
            RectTransform rt = backImg.rectTransform;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}