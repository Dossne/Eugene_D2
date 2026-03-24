using System;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Tutorial
{
    public class TutorialButton : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Button button;
        private Action callback;

        public void Setup(Action callback)
        {
            this.callback = callback;
            button.onClick.AddListener(ExecuteCallback);
        }

        public void Dispose()
        {
            button.onClick.RemoveAllListeners();
            callback = null;
            Destroy(gameObject);
        }

        private void ExecuteCallback() 
        {
            callback?.Invoke();
        }
    }
}