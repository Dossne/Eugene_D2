#if UNITY_EDITOR

using System;
using Newtonsoft.Json;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class StorageService
    {
        private static readonly Lazy<StorageService> _lazyInstance =
            new Lazy<StorageService>(() => new StorageService());
        public static StorageService Instance => _lazyInstance.Value;
        private StorageData _storageData = new StorageData();
        private readonly string _fileName = "saykit.json";
        private FileSystemService _fileSystemService;

        private StorageService()
        {
            _fileSystemService = new FileSystemService(_fileName);
            InitDataFromFile();
        }

        public bool IsPremium
        {
            get => _storageData.IsPremium;
            set => _storageData.IsPremium = value;
        }
        
        public bool RateAppPopupWasShowed
        {
            get => _storageData.RateAppPopupWasShowed;
            set => _storageData.RateAppPopupWasShowed = value;
        }

        private void InitDataFromFile()
        {
            try
            {
                if (_fileSystemService.IsFileExist())
                {
                    var json = _fileSystemService.ReadFile();
                    var data = JsonConvert.DeserializeObject<StorageData>(json);

                    if (data != null)
                    {
                        _storageData = data;
                    }
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"[StorageService] InitDataFromFile message: {e.Message}");
            }
        }

        public void Save()
        {
            _fileSystemService.SaveData(_storageData);
        }
    }
}
#endif