using Febucci.UI;
using Infrastructure.Utilities;
using UnityEngine;

namespace Features.InfoPopup
{
    public class IPTypewriterByCharacterBhvr : IPAnimationBehaviour
    {
        [SerializeField] private TypewriterByCharacter typewriter;

        public override void Initialize()
        {
            
        }


        public override void Deinitialize()
        {
            
        }


        public override void Prepare()
        {
            typewriter.gameObject.SetObjectActive(false);
        }


        public override void Play()
        {
            typewriter.gameObject.SetObjectActive(true);
            typewriter.StartShowingText(true);
        }


        public override void Stop()
        {
            typewriter.StopDisappearingText();
        }
    }
}