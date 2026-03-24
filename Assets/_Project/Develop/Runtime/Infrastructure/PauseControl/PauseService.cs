using Infrastructure.DateTimeControl;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.PauseControl
{
    public class PauseService
    {
        private readonly HashSet<object> pauseInitiators = new();


        public bool Pause(object pauseInitiator)
        {

            bool isPausedNow = false;
            if (pauseInitiators.Count == 0)
            {
                Time.timeScale = 0;
                isPausedNow = true;
            }

            pauseInitiators.Add(pauseInitiator);

            return isPausedNow;
        }


        public bool Resume(object pauseInitiator)
        {
            bool isResumeNow = false;
            if (pauseInitiators.Count == 0)
            {
                return isResumeNow;
            }

            pauseInitiators.Remove(pauseInitiator);

            if (pauseInitiators.Count == 0)
            {
                Time.timeScale = DateTimeService.DefaultTimeScale;
                isResumeNow = true;
            }

            return isResumeNow;
        }
    }
}