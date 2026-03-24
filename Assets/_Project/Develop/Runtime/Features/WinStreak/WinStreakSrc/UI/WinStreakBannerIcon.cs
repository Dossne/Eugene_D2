using UnityEngine;
using UnityEngine.UI;

namespace Features.WinStreak
{
    public class WinStreakBannerIcon : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image iconImg;
        [SerializeField] private GameObject particleBg;
        [SerializeField] private GameObject particleFg;
        [SerializeField] private WinStreakBannerAnimator anim;

        [Header("States")]
        [SerializeField] private Sprite closeIcon;
        [SerializeField] private Sprite openIcon;


        public void SetClosed()
        {
            iconImg.sprite = closeIcon;
            anim.SetAnimatorEnabled(false);
            SetFxActive(false);
        }


        public void SetOpened()
        {
            iconImg.sprite = openIcon;
            anim.SetAnimatorEnabled(false);
            SetFxActive(false);
        }


        public void PlayOpen()
        {
            anim.SetAnimatorEnabled(true);
            SetFxActive(false);
            anim.PlayOpenProcess();
        }


        public void PlayOpenFx()
        {
            SetFxActive(true);
        }

        public void ActivateBgFx()
        {
            SetObjectActive(particleBg, true);
        }
        

        public void SetObjectActive(bool value)
        {
            SetObjectActive(gameObject, value);
        }


        private void SetFxActive(bool value)
        {
            SetObjectActive(particleBg, value);
            SetObjectActive(particleFg, value);
        }


        private void SetObjectActive(GameObject obj, bool value)
        {
            if (obj.activeSelf == value)
                return;

            obj.SetActive(value);
        }
    }
}