using System;

namespace Features.Tutorial
{
    public abstract class TutorialBase 
    {
        public Action<string> OnComplete;        

        public string TutorialId { get; private set; }

        public TutorialBase(string tutorialId) 
        {
            TutorialId = tutorialId;
        }



        /// <summary>
        /// Do not use the method inside tutorial! 
        /// The method is used to verify whether it is possible to start a specific tutorial in current moment.
        /// The check must include feature enable, unlock level, scene and other parameters which must block tutorial start in current moment, 
        /// i.e. required popup triggered enqueue of this tutorial but could be closed by previous tutorial with the same trigger.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanBeStarted();

        /// <summary>
        /// Do not use the method inside tutorial! 
        /// The method is used to verify whether it is required to enqueue a specific tutorial by trigger.
        /// The check must include feature enable, unlock level, scene and other parameters which block tutorial start in general, 
        /// i.e. player has already passed strictly required level before tutorial added in game.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanBeEqueued();

        public abstract void Start();
        public abstract void Stop();
    }
}