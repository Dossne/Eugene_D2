using UnityEngine;

namespace Infrastructure.Utilities
{
    public static class GameObjectExtensions
    {
        public static void SetObjectActive(this GameObject obj, bool value)
        {
            try
            {
                if (obj.activeSelf == value)
                    return;

                obj.SetActive(value);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}