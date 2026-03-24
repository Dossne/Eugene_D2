using System.Collections.Generic;
using Infrastructure.Localization;
using UnityEngine;

namespace Features.InfoPopup
{
    public class InfoPopupElementBlock : MonoBehaviour
    {
        [SerializeField] private List<TMPLocalizer> tmpLocalizers;
        [SerializeField] private List<IPAnimationBehaviour> animations;


        public void Initialize()
        {
            foreach (TMPLocalizer localizer in tmpLocalizers)
            {
                if (localizer.txtItem == null)
                {
                    Debug.LogError("Localizer is null", this);
                    continue;
                }
                localizer.Localize();
            }

            foreach (IPAnimationBehaviour anim in animations)
            {
                if (anim == null)
                {
                    Debug.LogError("Animation is null", this);
                    continue;
                }
                
                anim.Initialize();
            }
        }


        public void Deinitialize()
        {
            foreach (IPAnimationBehaviour anim in animations)
            {
                anim.Deinitialize();
            }
        }


        public void Prepare()
        {
            foreach (IPAnimationBehaviour anim in animations)
            {
                anim.Prepare();
            }
        }


        public void PlayAnimation()
        {
            foreach (IPAnimationBehaviour anim in animations)
            {
                anim.Play();
            }
        }


        public void StopAnimation()
        {
            foreach (IPAnimationBehaviour anim in animations)
            {
                anim.Stop();
            }
        }

    }
}