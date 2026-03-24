using System.IO;
using UnityEngine;

namespace Infrastructure.PersistentProgress
{
    public class SaveConstants
    {
        public const string FileExtension = "json";
        public const string EncryptKey = "f5bc2d6a-06dc-4c11-af03-08f97b7e6a37";
        private const string FolderName = "saves/";
        private const string MainFileName = "main";


        public static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, FolderName);
        }


        public static string GetMainFilePath()
        {
            return Path.Combine(GetSaveDirectory(), MainFileName);
        }


        public static string GetFilePath(string fileName)
        {
            return Path.Combine(GetSaveDirectory(), fileName);
        }


        public static void CreateSaveDirectory()
        {
            string saveDirectory = GetSaveDirectory();

            CreateDirectoryIfNotExists(saveDirectory);
        }


        public static void CreateDirectoryIfNotExists(string saveDirectory)
        {
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }


        public static string GetFileName()
        {
            return $"{MainFileName}.{FileExtension}";
        }


        #region Cheat

#if PR_CHEAT
        
        public static string GetCheatSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "cheat_saves/");
        }
#endif

        #endregion

    }
}