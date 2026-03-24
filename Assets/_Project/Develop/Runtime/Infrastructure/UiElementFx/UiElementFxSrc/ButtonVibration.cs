using Infrastructure.AudioControl;
using Infrastructure.HapticControl;
using R3;
using UnityEngine;
using UnityEngine.UI;


namespace Infrastructure.UiElementFx
{
    [RequireComponent(typeof(Button))]
    public class ButtonVibration : MonoBehaviour
    {
        [SerializeField] private Button targetButton;
        [SerializeField] private HapticType hapticOnClick = HapticType.Selection;
        [SerializeField] private SfxType sfx = SfxType.ClickUI;
        
        private CompositeDisposable disposables = new();

        private void OnEnable()
        {
            if (targetButton == null)
                return;

            disposables = new();
            targetButton.OnClickAsObservable().Subscribe(_=>PlayOnClick()).AddTo(disposables);
        }

        private void OnDisable()
        {
            if (targetButton == null)
                return;

            disposables.Dispose();
        }

        private void PlayOnClick()
        {
            HapticService.I.Haptic(hapticOnClick);
            AudioService.I.PlaySfx(sfx);
        }
    }
}