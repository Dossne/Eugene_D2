using Infrastructure.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Features.LavaQuest
{
    public class LavaQuestPlatform : MonoBehaviour
    {
        private const string LavaMove = "LavaMove";
        private const string Default = "Default";

        [SerializeField] private int step;
        [SerializeField] private RectTransform itemPlaceRect;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Image visualRootImage;
        [SerializeField] private Animator anim;
        [SerializeField] private bool isMoveInLava = true;

        public RectTransform ItemPlaceRect => itemPlaceRect;
        public Image VisualRootImage => visualRootImage;
        public int Step => step;
        public bool IsMoveInLava => isMoveInLava;


        public void Initialize()
        {
            if (anim != null)
                anim.enabled = false;
        }


        public void SetObjectActive(bool value)
        {
            visualRoot.SetObjectActive(value);
        }


        public void ResetState()
        {
            if (anim != null)
            {
                anim.enabled = true;
                anim.Play(Default, 0, 0);
            }
        }


        public void AnimateMoveDown()
        {
            if (anim != null && gameObject.activeInHierarchy)
            {
                anim.enabled = true;
                anim.Play(LavaMove,0, 0);
            }
            
            else if (anim == null)
            {
                Debug.LogWarning($"[LavaQuest] Can't play {LavaMove} animation. Add Animator for platform step: {step}", this);
            }
        }
    }
}