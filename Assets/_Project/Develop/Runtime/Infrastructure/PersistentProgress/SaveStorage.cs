using Infrastructure.Events;
using R3;

namespace Infrastructure.PersistentProgress
{
    public class SaveStorage
    {
        private JsonFileSaveLoader saveLoader;
        private string fileName;

        public Progress Progress { get; private set; }

        private SoftResetEvent softResetEvent;

        public bool IsInitialized { get; private set; } = false;

        public SaveStorage(SoftResetEvent softResetEvent)
        {
            this.softResetEvent = softResetEvent;
        }


        public void Initialize()
        {
            SaveConstants.CreateSaveDirectory();
            fileName = SaveConstants.GetFileName();
            saveLoader = new JsonFileSaveLoader(SaveConstants.EncryptKey);
            Load();
            IsInitialized = true;
        }


        public bool ApplyProgress(string json, bool jsonEncryptedAES = true, bool encryptAES = true)
        {
            if (!saveLoader.ApplyFromJson<Progress>(json, fileName, jsonEncryptedAES, encryptAES))
            {
                return false;
            }

            softResetEvent.Execute(Unit.Default);
            return true;
        }


        public void ResetProgress()
        {
            Progress = new Progress();
            Save();
            softResetEvent.Execute(Unit.Default);
        }


        private void Load()
        {
            Progress = saveLoader.Load(fileName, new Progress());
        }


        public void Save()
        {
            saveLoader.Save(fileName, Progress);
        }
    }
}