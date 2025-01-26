using System;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace EGS.Utils 
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T m_instance;

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<T>();

                    if (m_instance == null)
                    {
                        m_instance = new GameObject(
                            typeof(T).Name, typeof(T)
                        ).GetComponent<T>();
                    }

                    if (m_instance == null)
                        throw new NullReferenceException("Component of Type " + typeof(T).Name + "  not Found in Scene " + SceneManager.GetActiveScene().name);
                }

                return m_instance;
            }
        }

        public virtual bool IsDontDestroyOnLoad => true;

        protected virtual void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_instance = (T)this;

            if (IsDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

    }
}
