using UnityEngine;

namespace Features.WinStreak
{
    public class WinStreakBannerAnimator : MonoBehaviour
    {
        private static int OpenProcessHash = Animator.StringToHash("OpenProcess");

        [SerializeField] private Animator anim;
        [SerializeField] private WinStreakBannerIcon parent;


        public void SetAnimatorEnabled(bool value)
        {
            anim.enabled = value;
        }


        public void PlayOpenProcess()
        {
            anim.Play(OpenProcessHash, 0, 0);
        }


        public void AnimationEnded_AnimEvt()
        {
            SetAnimatorEnabled(false);
        }


        public void PlayFx_AnimEvt()
        {
            parent.PlayOpenFx();
        }
    }
}