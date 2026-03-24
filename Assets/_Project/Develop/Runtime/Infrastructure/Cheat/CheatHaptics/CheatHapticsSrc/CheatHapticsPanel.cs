#if PR_CHEAT
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Infrastructure.HapticControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Infrastructure.Cheat
{
    public class CheatHapticsPanel : MonoBehaviour
    {
        [SerializeField] private List<Button> closeBtns;
        [SerializeField] private Button actionButtonPf;
        [SerializeField] private RectTransform buttonGridRoot;
        [Header("Actions")]
        [SerializeField] private Button constantBtn;

        [Header("Amplitude")]
        [SerializeField] private Slider amplitudeSlider;
        [SerializeField] private TextMeshProUGUI amplitudeValue;

        [Header("Duration")]
        [SerializeField] private Slider durationSlider;
        [SerializeField] private TextMeshProUGUI durationValue;

        private bool isInit;
        private List<Button> presetButtons = new();


        public void Initialize()
        {
            if (isInit)
                return;


            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            InitCustom();
            InitPresets();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveAllListeners();
            }

            foreach (var btn in presetButtons)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }

            presetButtons.Clear();

            amplitudeSlider.onValueChanged.RemoveAllListeners();
            durationSlider.onValueChanged.RemoveAllListeners();
            isInit = false;
        }


        public void Open()
        {
            gameObject.SetObjectActive(true);
        }


        private void Close()
        {
            gameObject.SetObjectActive(false);
        }


        private void InitCustom()
        {
            amplitudeSlider.value = 0.5f;
            SetAmpText(amplitudeSlider.value);
            durationSlider.value = 0.1f;
            SetDurText(durationSlider.value);

            constantBtn.onClick.AddListener(ConstantClick);
            amplitudeSlider.onValueChanged.AddListener(AmpSlider_OnValueChanged);
            durationSlider.onValueChanged.AddListener(DurSlider_OnValueChanged);
        }


        private void InitPresets()
        {
            var enums = Enum.GetValues(typeof(HapticType)).Cast<HapticType>();
            foreach (HapticType hType in enums)
            {
                if (hType is HapticType.None)
                    continue;

                var btnInstance = Instantiate(actionButtonPf, buttonGridRoot);
                btnInstance.gameObject.SetObjectActive(true);
                var txt = btnInstance.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = hType.ToString().SplitCamelCase();
                btnInstance.onClick.AddListener(() => HapticService.I.Haptic(hType));
            }
        }


        private void SetAmpText(float val)
        {
            amplitudeValue.text = val.ToString("F2", CultureInfo.InvariantCulture);
        }


        private void SetDurText(float val)
        {
            durationValue.text = val.ToString("F2", CultureInfo.InvariantCulture);
        }


        private void ConstantClick()
        {
            HapticService.I.CheatHapticNoBake(amplitudeSlider.value, durationSlider.value);
        }


        private void AmpSlider_OnValueChanged(float val)
        {
            SetAmpText(val);
        }


        private void DurSlider_OnValueChanged(float val)
        {
            SetDurText(val);
        }
    }
}

#endif