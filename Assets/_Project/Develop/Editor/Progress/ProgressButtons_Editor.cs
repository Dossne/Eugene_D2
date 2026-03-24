using System.IO;
using Infrastructure.PersistentProgress;
using UnityEditor;
using UnityEngine;

namespace Progress
{
    public static class ProgressButtons_Editor
    {
#if UNITY_EDITOR
        private const string Tag = "[Save-Load]";
        private const string ButtonPath = "Tools/Save-Load/";

        [MenuItem(ButtonPath + "1.Open folder")]
        public static void OpenFileFolder()
        {
            string path = SaveConstants.GetSaveDirectory();
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem(ButtonPath + "2.Decrypt save")]
        public static void DecryptSave()
        {
            string directory = SaveConstants.GetSaveDirectory();
            string extension = SaveConstants.FileExtension;

            string sourceFilePath = EditorUtility.OpenFilePanel("Select save file to decrypt", directory, extension);

            if (sourceFilePath.Length == 0)
            {
                Debug.Log($"{Tag} Decrypt aborted. Source file not selected");
                return;
            }

            string sourceWithoutExtenstion = Path.GetFileNameWithoutExtension(sourceFilePath);
            
            string outputFilePath = EditorUtility.SaveFilePanel("Set decrypted file name", directory, sourceWithoutExtenstion + "_decrypted", SaveConstants.FileExtension);

            if (outputFilePath.Length == 0)
            {
                Debug.Log($"{Tag} Decrypt aborted. Name for decrypted file not set");
                return;
            }

            string outputFileName = Path.GetFileName(outputFilePath);

            JsonFileSaveLoader saveLoader = new JsonFileSaveLoader(SaveConstants.EncryptKey);
            string sourceFileNameFull = Path.GetFileName(sourceFilePath);

            saveLoader.DecryptFile(sourceFileNameFull, outputFileName, new Infrastructure.PersistentProgress.Progress());
            Debug.Log($"{Tag} Decrypt success. File \"{sourceFileNameFull}\" decrypted into \"{outputFilePath}\"");

            OpenFileFolder();
        }

        [MenuItem(ButtonPath + "3.Encrypt save")]
        public static void EncryptSave()
        {
            string directory = SaveConstants.GetSaveDirectory();
            string extension = SaveConstants.FileExtension;

            string sourceFilePath = EditorUtility.OpenFilePanel("Select save file to encrypt", directory, extension);

            if (sourceFilePath.Length == 0)
            {
                Debug.Log($"{Tag} Encrypt aborted. Source file not selected");
                return;
            }

            string sourceFileName = Path.GetFileName(sourceFilePath);
            string outputFileName = SaveConstants.GetFileName();
            string outputFilePath = SaveConstants.GetMainFilePath();

            if (File.Exists(outputFilePath) && !EditorUtility.DisplayDialog("Save", $"File \"{outputFileName}\" already exists. Overwrite?", "Ok", "Cancel"))
            {
                return;
            }

            JsonFileSaveLoader saveLoader = new JsonFileSaveLoader(SaveConstants.EncryptKey);
            saveLoader.EncryptFile(sourceFileName, outputFileName, new Infrastructure.PersistentProgress.Progress());

            Debug.Log($"{Tag} Encrypt success. File \"{sourceFileName}\" encrypted into \"{outputFilePath}\"");

            OpenFileFolder();
        }

#endif
    }
}