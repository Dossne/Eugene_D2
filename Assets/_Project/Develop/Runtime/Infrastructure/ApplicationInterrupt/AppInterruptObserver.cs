using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.ApplicationInterrupt
{
    public class AppInterruptObserver : MonoBehaviour
    {
        public event Action Interrupt;
        public event Action Resume;

        public event Action<bool> OnFocus;

        private enum InterruptingType
        {
            LostFocus,
            ApplicationPause,
            ApplicationQuit
        }

        private readonly HashSet<InterruptingType> appInterrupted = new();

        public bool IsInterrupted => appInterrupted.Count > 0;


        private void OnApplicationPause(bool paused)
        {
            SetGameInterrupt(InterruptingType.ApplicationPause, paused);
        }


        private void OnApplicationFocus(bool focus)
        {
            OnFocus?.Invoke(focus);
            SetGameInterrupt(InterruptingType.LostFocus, !focus);
        }


#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            SetGameInterrupt(InterruptingType.ApplicationQuit, isInterrupted: true);
        }
#endif


        private void SetGameInterrupt(InterruptingType interruptingType, bool isInterrupted)
        {
            if (isInterrupted)
            {
                if (appInterrupted.Count == 0)
                {
                    Interrupt?.Invoke();
                }

                appInterrupted.Add(interruptingType);
            }
            else
            {
                appInterrupted.Remove(interruptingType);

                if (appInterrupted.Count == 0)
                {
                    Resume?.Invoke();
                }
            }
        }
    }
}