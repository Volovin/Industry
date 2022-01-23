using System;
using System.Collections.Generic;
using UnityEngine;

namespace Industry
{
    /// <summary>
    /// The base class that implements the Singleton pattern for a specified type <typeparamref name="T"/>.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Constructors

        protected Singleton()
        {

        }

        #endregion

        #region Fields

        private static bool m_ShuttingDown = false;
        private static T m_Instance;

        #endregion

        #region Properties

        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                if (m_Instance == null)
                {
                    // Search for existing instance.
                    m_Instance = (T)FindObjectOfType(typeof(T));
                    
                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        //singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    }
                }

                return m_Instance;
            }
        }

        #endregion

        #region Unity methods

        private void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }
        
        private void OnDestroy()
        {
            m_ShuttingDown = true;
        }

        #endregion
    }
}