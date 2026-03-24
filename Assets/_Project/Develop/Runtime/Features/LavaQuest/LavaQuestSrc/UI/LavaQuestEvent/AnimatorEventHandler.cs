using UnityEngine;

namespace Features.LavaQuest
{
    public class AnimatorEventHandler : MonoBehaviour
    {
        [SerializeField] private Animator anim;


        /// <summary>
        /// Animator event
        /// </summary>
        public void DisableSelf_AnimEvt()
        {
            anim.enabled = false;
        }
    }
}