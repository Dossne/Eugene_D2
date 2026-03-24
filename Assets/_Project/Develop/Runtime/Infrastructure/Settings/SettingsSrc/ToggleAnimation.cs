using UnityEngine;

public class ToggleAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateOn = "ON";
    [SerializeField] private string stateOff = "OFF";


    public void PlayAnimation(bool isOn)
    {
        if (gameObject.activeSelf)
        {
            animator.Play(isOn ? stateOn : stateOff);
        }
    }
}
