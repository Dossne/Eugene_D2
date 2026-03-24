using System.Collections.Generic;
using Features.Collectables;
using Infrastructure.Utilities;
using Newtonsoft.Json;

namespace Features.LevelConfiguration
{
    public class LevelUtil
    {
        public static bool TryGetLevel(string levelId, string compressedJson, out Level level)
        {
            bool isSuccess = true;
            try
            {
                if(string.IsNullOrEmpty(compressedJson))
                    throw new System.ArgumentException("compressedJson cannot be null or empty");
                
                string rawJson = StringUtils.DecompressString(compressedJson);
                level = JsonConvert.DeserializeObject<Level>(rawJson, JsonUtils.SerializerSettings);
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.LogError($"[LevelConfig] Level Id: \"{levelId}\". Error parse LEVEL json");
                level = null;
                isSuccess = false;
            }
            
            return isSuccess;
        }
        
        
        public static bool TryGetTasks(string levelId, string taskJson, out List<CollectableType> tasks)
        {
            bool isSuccess = true;
            try
            {
                if(string.IsNullOrEmpty(taskJson))
                    throw new System.ArgumentException("taskJson cannot be null or empty");
                
                tasks = JsonConvert.DeserializeObject<List<CollectableType>>(taskJson, JsonUtils.SerializerSettings);
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.LogError($"[LevelConfig] Level Id: \"{levelId}\". Error parse TASK json");
                tasks = null;
                isSuccess = false;
            }
            
            return isSuccess;
        }
    }
}