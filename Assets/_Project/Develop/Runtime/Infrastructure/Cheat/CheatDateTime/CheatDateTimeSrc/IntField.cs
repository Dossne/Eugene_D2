using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Cheat
{
    public enum ClampType
    {
        None = 0,
        ClampMinMax = 1,
        CycleMinMax = 2,
    }

    public class IntField : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button plusBtn;
        [SerializeField] private Button minusBtn;
        [SerializeField] private TextMeshProUGUI stateTxt;
        [SerializeField] private TextMeshProUGUI labelTxt;

        [Header("Config")]
        [SerializeField] private string labelName;
        [SerializeField] private int initialValue;
        [SerializeField] private int step;
        [SerializeField] private ClampType clampType;
        [SerializeField] private int valueMin;
        [SerializeField] private int valueMax;

        private int currentValue;
        private bool isInit;

        public int CurrentValue => currentValue;

#if UNITY_EDITOR

        private void OnValidate()
        {
            labelTxt.text = labelName;
            Set(initialValue);
            RefreshText();
        }
#endif


        public void Initialize()
        {
            if (isInit)
                return;

            labelTxt.text = labelName;
            Set(initialValue);
            RefreshText();

            plusBtn.onClick.AddListener(AddStep);
            minusBtn.onClick.AddListener(DecreaseStep);

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            plusBtn.onClick.RemoveAllListeners();
            minusBtn.onClick.RemoveAllListeners();

            isInit = false;
        }


        public void Set(int value)
        {
            ClampedChange(value, ActionType.Set);
            RefreshText();
        }


        private void AddStep()
        {
            ClampedChange(step, ActionType.Add);
            RefreshText();
            Vibrate();
        }


        private void DecreaseStep()
        {
            ClampedChange(step, ActionType.Remove);
            RefreshText();
            Vibrate();
        }


        private void Vibrate()
        {
            HapticControl.HapticService.I.HapticLight();
        }


        private void ClampedChange(int value, ActionType action)
        {
            switch (action)
            {
                case ActionType.Add:
                    currentValue += value;
                    break;
                case ActionType.Remove:
                    currentValue -= value;
                    break;
                case ActionType.Set:
                    currentValue = value;
                    break;
            }

            switch (clampType)
            {
                case ClampType.None:
                    return;
                case ClampType.ClampMinMax:
                    currentValue = Mathf.Clamp(currentValue, valueMin, valueMax);
                    break;
                case ClampType.CycleMinMax:
                    if (currentValue < valueMin)
                        currentValue = valueMax;
                    else if (currentValue > valueMax)
                        currentValue = valueMin;
                    break;
            }
        }


        private void RefreshText()
        {
            stateTxt.text = currentValue.ToString();
        }

    }
}