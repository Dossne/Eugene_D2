using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Boosters
{
    public class CounterView : MonoBehaviour
    {
        [Header("Main components")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI text;

        [Header("States")]
        [SerializeField] private SerializedDictionary<BoosterStateType, Sprite> bgSprites;
        [SerializeField] private SerializedDictionary<BoosterStateType, GameObject> foregroundGo;


        public void SetState(BoosterStateType state, string text)
        {
            if (bgSprites.TryGetValue(state, out Sprite bgIcon))
            {
                background.sprite = bgIcon;
                SetBackgroundImageActive(true);
            }
            else
            {
                SetBackgroundImageActive(false);
            }

            foreach (var entryPair in foregroundGo)
            {
                bool active = entryPair.Key == state;

                if (entryPair.Value.gameObject.activeSelf != active)
                    entryPair.Value.gameObject.SetActive(active);
            }

            this.text.text = text;
        }


        private void SetBackgroundImageActive(bool value)
        {
            if (background.gameObject.activeSelf == value)
                return;

            background.gameObject.SetActive(value);
        }
    }
}