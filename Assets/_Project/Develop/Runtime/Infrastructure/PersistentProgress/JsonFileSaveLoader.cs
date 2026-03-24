using Infrastructure.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Infrastructure.PersistentProgress
{
    public class JsonFileSaveLoader
    {
        private static string seed = "seed";


        public JsonFileSaveLoader(string _seed)
        {
            seed = _seed;
        }


        public T Load<T>(string fileName, T defaultData, bool decryptAES = true)
        {
            string filePath = SaveConstants.GetFilePath(fileName);

            if (!File.Exists(filePath))
                return defaultData;

            return LoadFromPath(filePath, defaultData, decryptAES);
        }


        public bool Save<T>(string fileName, T data, bool encryptAES = true)
        {
            string filePath = SaveConstants.GetFilePath(fileName);
            return SaveToPath(filePath, data, encryptAES);
        }


        public T LoadFromPath<T>(string fullPathWithFileName, T defaultData, bool decryptAES = true)
        {
            using StreamReader file = new StreamReader(fullPathWithFileName);
            string jsonData = decryptAES ? DecryptAES(file.ReadToEnd()) : file.ReadToEnd();
            T saveData = JsonConvert.DeserializeObject<T>(jsonData, JsonUtils.SerializerSettings);
            return saveData == null ? defaultData : saveData;
        }


        public bool SaveToPath<T>(string fullPathWithFileName, T data, bool encryptAES = true)
        {
            string jsonData = JsonConvert.SerializeObject(data, JsonUtils.SerializerSettings);
            
            try
            {
                using var file = new StreamWriter(fullPathWithFileName);
                file.Write(encryptAES ? EncryptAES(jsonData) : jsonData);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[JsonFileSaveLoader] Save error: {e.GetType()} {e.Message}");
                return false;
            }
            
            return true;
        }


        public bool ApplyFromJson<T>(string fromJson, string toFileName, bool jsonEncryptedAES = true, bool encryptAES = true)
        {
            string jsonData = jsonEncryptedAES ? DecryptAES(fromJson) : fromJson;
            T saveData = JsonConvert.DeserializeObject<T>(jsonData, JsonUtils.SerializerSettings);

            if (saveData == null)
                return false;

            return Save(toFileName, saveData, encryptAES);
        }


        public void Delete(string fileName)
        {
            string filePath = SaveConstants.GetFilePath(fileName);

            if (!File.Exists(filePath))
                return;

            File.Delete(filePath);
        }


        public void DecryptFile<T>(string sourceFileName, string outputFileName, T defaultData)
        {
            T encryptedState = Load(sourceFileName, defaultData);
            Save(outputFileName, encryptedState, false);
        }


        public void EncryptFile<T>(string sourceFileName, string outputFileName, T defaultData)
        {
            T decryptedState = Load(sourceFileName, defaultData, false);
            Save(outputFileName, decryptedState);
        }


        private static byte[] GenerateIVBytes()
        {
            byte[] ivBytes = new byte[16];
            System.Random rnd = new System.Random();
            rnd.NextBytes(ivBytes);
            return ivBytes;
        }


        private static byte[] GenerateKeyBytes()
        {
            byte[] keyBytes = new byte[16];
            int sum = 0;
            foreach (char curChar in seed)
                sum += curChar;

            System.Random rnd = new System.Random(sum);
            rnd.NextBytes(keyBytes);
            return keyBytes;
        }


        private static string EncryptAES(string data)
        {
            byte[] ivBytes = GenerateIVBytes();
            byte[] keyBytes = GenerateKeyBytes();

            SymmetricAlgorithm algorithm = Aes.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(keyBytes, ivBytes);
            byte[] inputBuffer = Encoding.Unicode.GetBytes(data);
            byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);

            string ivString = Convert.ToBase64String(ivBytes);
            string encryptedString = Convert.ToBase64String(outputBuffer);

            return ivString + "." + encryptedString;
        }


        private static string DecryptAES(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.IndexOf(".") == -1)
            {
                UnityEngine.Debug.LogException(new($"Save file is damaged, progress lost. Damaged content={text}"));
                return string.Empty;
            }                

            byte[] keyBytes = GenerateKeyBytes();
            string ivString = text.Substring(0, text.IndexOf("."));
            byte[] extractedivBytes = Convert.FromBase64String(ivString);

            string encryptedString = text.Substring(ivString.Length + 1);

            SymmetricAlgorithm algorithm = Aes.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(keyBytes, extractedivBytes);
            byte[] inputBuffer = Convert.FromBase64String(encryptedString);
            byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);

            string decryptedString = Encoding.Unicode.GetString(outputBuffer);

            return decryptedString;
        }
    }
}