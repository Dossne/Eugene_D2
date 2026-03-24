using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace

#endregion

namespace SayKitInternal
{
    public class FileSystemService
    {
        private readonly object _lockObject = new object();
        private readonly string _filePath;
        private const string LogTag = "[FileSystemService]";

        public FileSystemService(string fileName)
        {
            _filePath = Path.Combine(Application.persistentDataPath, fileName);
        }
        
        public void SaveData<T>(T data)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(data);
                if (jsonData.Length == 0)
                {
                    SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: "[FileSystemService]: " + "File: " + _filePath + " Exception: data is empty.");
                }
                else
                {
                    SaveFile(jsonData);
                }
            }
            catch (Exception e)
            {
                var exceptionMessage = "[FileSystemService]: SaveData " + "File: " + _filePath + " Exception: " + e.Message;
                SayKitDebug.LogError(exceptionMessage);
                SKBridgeManager.Instance.TrackEvent(name: "sk_unity_exception", extra1: exceptionMessage);
            }
        }
        
        private void SaveFile(string data)
        {
            try
            {
                lock (_lockObject)
                {
                    CheckIfFileExist();
                    File.WriteAllText(_filePath, data);
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{LogTag}[SaveFile] exception: {e}");
            }
        }

        public void SaveFile(byte[] data)
        {
            try
            {
                lock (_lockObject)
                {
                    CheckIfFileExist();
                    File.WriteAllBytes(_filePath, data);
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{LogTag}[SaveFile] exception: {e}");
            }
        }
        
        public string ReadFile()
        {
            var fileString = string.Empty;

            try
            {
                lock (_lockObject)
                {
                    fileString = File.ReadAllText(_filePath);
                }
            }
            catch (Exception e)
            {
                SKUtils.HandleError($"{LogTag}[ReadFile] ReadFile from {_filePath}, exception: {e}");
            }

            return fileString;
        }
        
        public bool IsFileExist()
        {
            return File.Exists(_filePath);
        }

        private void CheckIfFileExist()
        {
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
            }
        }
    }
}