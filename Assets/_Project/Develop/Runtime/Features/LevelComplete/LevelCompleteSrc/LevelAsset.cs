using UnityEngine;

public abstract class LevelAsset : ScriptableObject
{
    [SerializeField] private string levelId;
    public string LevelId => levelId;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = System.Guid.NewGuid().ToString();
        }
    }
}