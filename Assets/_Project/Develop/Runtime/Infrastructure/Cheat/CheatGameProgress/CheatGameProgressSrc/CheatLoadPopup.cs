#if PR_CHEAT
using System.Collections.Generic;
using System.IO;
using Infrastructure.PersistentProgress;
using Infrastructure.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Infrastructure.Cheat.CheatGameProgress
{
    public class CheatLoadPopup : PopupBase
    {
        private class SaveInfo
        {
            public string displayName;
            public string data;
        }

        [SerializeField] CheatLoadFolder cheatLoadFolderPf;
        [SerializeField] CheatLoadSlot cheatLoadSlotPrefab;
        [SerializeField] TextMeshProUGUI choosenSaveTmp;
        [SerializeField] Button loadButton;

        private SaveStorage saveStorage;
        private ToggleGroup toggleGroup;
        private Dictionary<string, List<SaveInfo>> saves = new();
        private string currentSave = string.Empty;


        [Inject]
        public void Construct(SaveStorage saveStorage)
        {
            this.saveStorage = saveStorage;
        }


        protected override void OnInitialize()
        {
            toggleGroup = GetComponentInChildren<ToggleGroup>();
            RemoveSaveList();
            BetterStreamingAssets.Initialize();
            loadButton.onClick.AddListener(LoadGame);
            GenerateSavesInfo();
            FillSavesList();
        }


        protected override void OnDeinitialize()
        {
            RemoveSaveList();
            loadButton.onClick.RemoveListener(LoadGame);
        }

        private void FillSavesList()
        {
            List<string> folders = new List<string>(saves.Keys);
            foreach (string folder in folders)
            {
                CheatLoadFolder cheatLoadFolder = Instantiate(cheatLoadFolderPf, toggleGroup.transform);
                List<GameObject> runTimeLoadSlotGO = new List<GameObject>();
                List<SaveInfo> savesInfo = saves[folder];
                foreach (SaveInfo saveInfo in savesInfo)
                {
                    CheatLoadSlot slot = Instantiate(cheatLoadSlotPrefab, toggleGroup.transform);
                    slot.Init(saveInfo.displayName, (value) =>
                    {
                        if (value)
                        {
                            choosenSaveTmp.text = saveInfo.displayName;
                            currentSave = saveInfo.data;
                        }
                        else
                        {
                            choosenSaveTmp.text = null;
                            currentSave = null;
                        }
                    });

                    runTimeLoadSlotGO.Add(slot.gameObject);
                }

                cheatLoadFolder.Init(folder, (value) => { runTimeLoadSlotGO.ForEach(go => go.SetActive(value)); });
            }
        }


        private void RemoveSaveList()
        {
            foreach (Transform child in toggleGroup.transform)
            {
                Toggle toggle = child.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.onValueChanged.RemoveAllListeners();
                }

                Destroy(child.gameObject);
            }
        }


        private void GenerateSavesInfo()
        {
            saves.Clear();

            string searchPattern = "*." + SaveConstants.FileExtension;

            string[] paths = BetterStreamingAssets.GetFiles("saves", searchPattern, SearchOption.AllDirectories);
            foreach (string path in paths)
            {
                if (path.Contains(".meta"))
                {
                    continue;
                }

                string jsonSave = string.Empty;
            
                using (Stream stream = BetterStreamingAssets.OpenRead(path))
                {
                    StreamReader reader = new StreamReader(stream);
                    jsonSave = reader.ReadToEnd();
                }

                SaveInfo saveInfo = new SaveInfo
                {
                    displayName = Path.GetFileName(path),
                    data = jsonSave
                };

                string folderName = Path.GetDirectoryName(path).Replace("saves", string.Empty);
                if (!saves.ContainsKey(folderName))
                {
                    saves.Add(folderName, new List<SaveInfo>());
                }

                saves[folderName].Add(saveInfo);
            }
        }


        private void LoadGame()
        {
            if (!string.IsNullOrEmpty(currentSave) && saveStorage.ApplyProgress(currentSave))
            {
                Close();
            }
            else
            {
                choosenSaveTmp.text = "File is not selected!";
            }
        }
    }
}
#endif