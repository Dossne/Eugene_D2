namespace Infrastructure.SystemModules
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "BuildNumberConfig", menuName = "Config/System/BuildNumberConfig")]
    public class BuildNumberConfig : ScriptableObject
    {
        public string buildNumber;
    }

}