using Features.Effects;
using UnityEngine;

namespace Infrastructure.CameraControl
{
    public class CameraVFX : MonoBehaviour
    {
        [SerializeField] private CameraVignette redVignette;
        [SerializeField] private FreezeVignette freezeVignette;


        public void ShowRedVignette(VignetteAnimFxData config, bool repeat)
        {
            var from = config.minAlpha;
            var to = config.maxAlpha;
            redVignette.Show(config.curve, from, to, config.duration, repeat);
        }


        public void HideRedVignette(bool IsAnimated)
        {
            if (IsAnimated)
            {
                redVignette.Hide();
            }
            else
            {
                redVignette.InstantHide();
            }
        }


        public void ShowFreezeVignette(VignetteAnimFxData config)
        {
            var from = config.minAlpha;
            var to = config.maxAlpha;
            freezeVignette.Show(config.curve, from, to, config.duration, false);
        }


        public void HideFreezeVignette(VignetteAnimFxData config, bool isAnimated)
        {
            if (isAnimated)
            {
                var from = config.maxAlpha;
                var to = config.minAlpha;
                freezeVignette.Hide(config.curve, from, to, config.duration);
            }
            else
            {
                freezeVignette.InstantHide();
            }
        }
    }
}