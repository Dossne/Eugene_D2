#if PR_CHEAT

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Features.LevelConfiguration;
using Infrastructure.Ads;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.CheatGameProgress
{
    public class CheatGameProgressPanel : MonoBehaviour
    {
        [SerializeField] private Button loadBtn;
        [SerializeField] private Button saveBtn;
        [SerializeField] private Button resetBtn;
        [SerializeField] private List<Button> closeBtns;

        private PopupService popupService;
        private CheatLoadPopup loadPopupInst;
        private CheatSavePopup savePopupInst;
        private SaveStorage saveStorage;
        private bool isInit;
        public bool IsInit => isInit;


        [Inject]
        public void Construct(PopupService popupService, SaveStorage saveStorage)
        {
            this.popupService = popupService;
            this.saveStorage = saveStorage;
        }


        public void Initialize()
        {
            if (isInit)
                return;

            foreach (var btn in closeBtns)
            {
                btn.onClick.AddListener(Close);
            }

            loadBtn.onClick.AddListener(OpenLoadPopup);

            saveBtn.onClick.AddListener(OpenSavePopup);
            resetBtn.onClick.AddListener(ResetProgress);

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
            loadBtn.onClick.RemoveListener(OpenLoadPopup);
            saveBtn.onClick.RemoveListener(OpenSavePopup);
            resetBtn.onClick.RemoveListener(ResetProgress);
           
            if (loadPopupInst != null)
                loadPopupInst.Deinitialize();

            if (savePopupInst != null)
                savePopupInst.Deinitialize();

            isInit = true;
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void Close()
        {
            gameObject.SetActive(false);
        }


        private async UniTaskVoid OpenLoadPopupAsync()
        {
            if (loadPopupInst == null)
            {
                loadPopupInst = await popupService.GetAsync<CheatLoadPopup>(gameObject.GetCancellationTokenOnDestroy(), inject: true);
                loadPopupInst.Initialize();
            }

            Close();
            loadPopupInst.Open();
        }


        private async UniTaskVoid OpenSavePopupAsync()
        {
            if (savePopupInst == null)
            {
                savePopupInst = await popupService.GetAsync<CheatSavePopup>(gameObject.GetCancellationTokenOnDestroy(), inject: true);
                savePopupInst.Initialize();
            }

            Close();
            savePopupInst.Open();
        }

        private void ResetProgress()
        {
            Advertisement.DisablePremium();
            saveStorage.ResetProgress();
        }
        
        private void OpenSavePopup()
        {
            OpenSavePopupAsync().Forget();
        }

        private void OpenLoadPopup()
        {
            OpenLoadPopupAsync().Forget();
        }
    }
}
#endif