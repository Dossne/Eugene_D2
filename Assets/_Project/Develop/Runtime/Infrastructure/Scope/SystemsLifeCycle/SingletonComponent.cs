using UnityEngine;

namespace Infrastructure.SystemsLifeCycle
{
    /// <summary>
    /// Use as less as possible
    /// </summary>
    public class SingletonComponent<T> : MonoBehaviour where T : Object
    {

        #region Variables

        private static T instance;

        #endregion

        #region Properties

        public static T I
        {
            get
            {
                // If the instance is null then either Instance was called to early or this object is not active.
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<T>();
                }

                if (instance == null)
                {
                    Debug.LogWarningFormat($"[SingletonComponent] Returning null instance for component of type {typeof(T)}");
                }

                return instance;
            }
        }

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            SetInstance();
        }

        #endregion

        #region Public Methods

        public static bool Exists()
        {
            return instance != null;
        }


        public bool SetInstance()
        {
            if (instance != null && instance != gameObject.GetComponent<T>())
            {
                Debug.LogWarning("[SingletonComponent] Instance already set for type " + typeof(T));
                return false;
            }

            instance = gameObject.GetComponent<T>();

            return true;
        }

        #endregion

    }

}