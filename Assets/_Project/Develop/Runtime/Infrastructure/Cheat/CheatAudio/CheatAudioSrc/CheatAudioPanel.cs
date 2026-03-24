#if PR_CHEAT

using System;
using System.Collections.Generic;
using System.Globalization;
using Infrastructure.AudioControl;
using Infrastructure.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Cheat
{
    public class CheatAudioPanel : MonoBehaviour
    {
        [SerializeField] private List<Button> closeBtns;
        [SerializeField] private TMP_Dropdown dropdown;

        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeValue;

        [SerializeField] private Button playBtn;
        [SerializeField] private Button stopMusicBtn;

        private SfxType currentSfxType;
        private bool isInit;


        public void Initialize()
        {
            if (isInit)
                return;


            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            InitDropDown();
            InitSlider();
            InitializeCurrentState();
            playBtn.onClick.AddListener(ClickPlayAudio);
            stopMusicBtn.onClick.AddListener(ClickStopMusicAudio);
            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.RemoveListener(Close);
            }

            volumeSlider.onValueChanged.RemoveListener(Slider_OnValueChanged);
            playBtn.onClick.RemoveListener(ClickPlayAudio);
            stopMusicBtn.onClick.RemoveListener(ClickStopMusicAudio);

            isInit = false;
        }


        public void Open()
        {
            gameObject.SetObjectActive(true);
        }


        private void InitSlider()
        {
            volumeSlider.value = 1f;
            Slider_OnValueChanged(1f);
            volumeSlider.onValueChanged.AddListener(Slider_OnValueChanged);
        }


        private void InitDropDown()
        {
            dropdown.options.Clear();
            var enums = Enum.GetValues(typeof(SfxType));

            int i = 0;
            foreach (SfxType val in enums)
            {
                if (val == SfxType.None)
                    continue;
                
                dropdown.options.Add(new TMP_Dropdown.OptionData(val.ToString()));
                
                if (i == 0)
                {
                    dropdown.captionText.text = val.ToString();
                }

                i++;
            }

            dropdown.onValueChanged.AddListener(DropdownOnValueChanged);
        }




        private void Close()
        {
            gameObject.SetObjectActive(false);
        }


        private void ClickPlayAudio()
        {
            AudioService.I.CheatPlay(currentSfxType, volumeSlider.value);
        }


        private void SetSliderValue(float value)
        {
            volumeValue.text = value.ToString("F2", CultureInfo.InvariantCulture);
        }


        private void InitializeCurrentState()
        {
            currentSfxType = Enum.Parse<SfxType>(dropdown.captionText.text);
            float volume = AudioService.I.GetVolume(currentSfxType);
            SetSliderValue(volume);
            volumeSlider.value = volume;
        }


        private void ClickStopMusicAudio()
        {
            AudioService.I.StopMusic();
        }


        private void Slider_OnValueChanged(float value)
        {
            SetSliderValue(value);
            AudioService.I.CheatSetVolume(currentSfxType, value);
        }


        private void DropdownOnValueChanged(int arg0)
        {
            InitializeCurrentState();
        }
    }
}

#endif