using System;
using TriInspector;
using UnityEngine;

namespace Features.InfoPopup
{
    [Serializable]
    public class InfoPopupElement
    {
        [Min(0)] public float playDelaySec;
        [SerializeField, Required] private InfoPopupElementBlock item;

        private float currentDelaySec;
        private bool isActive;


        public void Initialize()
        {
            item.Initialize();
        }


        public void Deinitialize()
        {
            item.Deinitialize();
            isActive = false;
        }


        public void Tick(float dt)
        {
            if (!isActive)
                return;

            currentDelaySec -= dt;

            if (currentDelaySec <= 0)
            {
                item.PlayAnimation();
                isActive = false;
            }
        }


        public void PlayAnimation()
        {
            currentDelaySec = playDelaySec;
            item.Prepare();
            isActive = true;
        }


        public void StopAnimation()
        {
            item.StopAnimation();
            isActive = false;
        }

    }
}