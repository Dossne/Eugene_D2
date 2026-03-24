using System;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Widgets
{
    public class Widget : MonoBehaviour, IWidgetReadable
    {
        public event Action OnChangeVisibility;

        [SerializeField] private WidgetId id;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject notifier;
        [SerializeField] private RectTransform scoringTarget;
        
        private Action callback;

        [field: SerializeField] public int Priority { get; private set; }
        [field: SerializeField] public RectTransform RectTransform { get; private set; }

        public Vector3 ScoringTargetOffset => scoringTarget.position - IconPosition;

        public WidgetId Id => id;

        public Vector3 IconPosition => icon.transform.position;
        public bool IsUnlocked { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;

        private void OnEnable()
        {
            OnChangeVisibility?.Invoke();
        }

        private void OnDisable()
        {
            OnChangeVisibility?.Invoke();
        }

        public void Construct(string text, Sprite sprite)
        {
            SetText(text);
            SetSprite(sprite);
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            button.onClick.AddListener(HandleClick);
            OnInitialize();
            IsInitialized = true;
        }

        public void Deinitialize()
        {
            if (!IsInitialized)
                return;

            callback = null;
            button.onClick.RemoveListener(HandleClick);
            OnDeinitialize();
            IsInitialized = false;
        }

        public void Dispose()
        {
            OnChangeVisibility = null;
            Destroy(gameObject);
        }

        public void Show()
        {
            SetObjectActive(true);
        }

        public void Hide()
        {
            SetObjectActive(false);
        }

        public void SetUnlocked(bool isUnlocked)
        {
            IsUnlocked = isUnlocked;
        }

        public void SetAction(Action callback)
        {
            this.callback = callback;
        }

        public void SetText(string text)
        {
            this.text.text = text;
        }

        public void SetNotifierActive(bool value)
        {
            if (notifier == null)
            {
                Debug.Log("Notifier is not set");
                return;
            }

            notifier.SetObjectActive(value);
        }

        public void SetObjectActive(bool active)
        {
            gameObject.SetObjectActive(active);
        }

        public void SetButtonEnabled(bool value)
        {
            button.enabled = value;
        }

        public void SetPriority(int priority)
        {
            Priority = priority;
        }

        private void SetSprite(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnDeinitialize() { }

        private void HandleClick()
        {
            callback?.Invoke();
        }
    }
}