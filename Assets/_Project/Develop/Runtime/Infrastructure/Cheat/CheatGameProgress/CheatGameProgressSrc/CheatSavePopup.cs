#if PR_CHEAT
using System.IO;
using System.Text.RegularExpressions;
using Infrastructure.BroTweens;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.CheatGameProgress
{
    public class CheatSavePopup : PopupBase
    {
        [SerializeField] private TMP_InputField saveFileNameInput;
        [SerializeField] private TextMeshProUGUI resultTMP;
        [SerializeField] private Button saveButton;

        private SaveStorage saveStorage;


        private void OnEnable()
        {
            Clear();
        }


        [Inject]
        public void Construct(SaveStorage saveStorage)
        {
            this.saveStorage = saveStorage;
        }


        protected override void OnInitialize()
        {
            saveButton.onClick.AddListener(SaveGameState);
            CheckAndRequestPermissions();
        }


        protected override void OnDeinitialize()
        {
            saveButton.onClick.RemoveListener(SaveGameState);
        }


        public void SetObjectActive(bool value)
        {
            if (gameObject.activeSelf == value)
                return;

            gameObject.SetActive(value);
        }


        private void CheckAndRequestPermissions()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
#endif
        }


        private void Clear()
        {
            saveFileNameInput.text = "";
            resultTMP.text = "";
        }


        private void SaveGameState()
        {
            string fileName = saveFileNameInput.text;
            if (!IsValidFileName(fileName))
            {
                ShowResult("Wrong file name", Color.red);
                return;
            }

            try
            {
                string savePath = GetSavePath(fileName, Application.platform == RuntimePlatform.Android);
                JsonFileSaveLoader jsonFileSaveLoader = new JsonFileSaveLoader(SaveConstants.EncryptKey);
                bool saveSuccess = jsonFileSaveLoader.SaveToPath(savePath, saveStorage.Progress);
                string result = saveSuccess ? $"Saved into:\n {savePath}" : "Didn't save. Something went wrong";
                Color textColor = saveSuccess ? Color.white : Color.red;
                ShowResult(result, textColor);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(savePath);
#endif
            }
            catch (System.Exception e)
            {
                ShowResult("Didn't save. Something went wrong", Color.red);
                UnityEngine.Debug.LogError($"[CheatSavePopup] Save error: {e.GetType()} {e.Message}]");
            }
        }


        private bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) 
                return false;
            
            string pattern = @"^[a-zA-Z0-9_\-а-яА-Я ]+$";
            return Regex.IsMatch(fileName, pattern);
        }


        private string GetSavePath(string fileName, bool androidSubFolder = false)
        {
            string directoryPath;

            if (Application.platform == RuntimePlatform.Android)
            {
                directoryPath = GetAndroidExternalFilesDir(androidSubFolder);
            }
            else
            {
                directoryPath = SaveConstants.GetCheatSaveDirectory();
            }

            SaveConstants.CreateDirectoryIfNotExists(directoryPath);
            return Path.Combine(directoryPath, $"{fileName}.json");
        }


        private string GetAndroidExternalFilesDir(bool isSubfolder = false)
        {
            using (AndroidJavaClass environmentClass = new AndroidJavaClass("android.os.Environment"))
            using (AndroidJavaObject downloadsDir = environmentClass.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory",
                                                                                                   environmentClass.GetStatic<string>("DIRECTORY_DOWNLOADS")))
            {
                string downloadPath = downloadsDir.Call<string>("getAbsolutePath");

                if (isSubfolder)
                    downloadPath = Path.Combine(downloadPath, GetFixedProductName() + "_cheat_saves");

                return downloadPath;
            }
        }


        private string GetFixedProductName()
        {
            string productName = Application.productName;
            string validPattern = "[^a-zA-Z0-9_-а-яА-Я]";
            return Regex.Replace(productName, validPattern, "");
        }


        private void ShowResult(string message, Color color)
        {
            resultTMP.color = color;
            resultTMP.text = message;
            ResultTMPAnimation();
        }


        private void ResultTMPAnimation()
        {
            BroTween.PunchScale(resultTMP.transform, Vector3.one * 0.2f, 0.3f, 1)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true)
                    .Play();
        }

    }
}
#endif