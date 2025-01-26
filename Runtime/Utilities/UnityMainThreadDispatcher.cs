using System;
using System.Collections.Generic;
using UnityEngine;


namespace EGS.Utils 
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;

        private readonly Queue<Action> _actions = new Queue<Action>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            while (_actions.Count > 0)
            {
                _actions.Dequeue().Invoke();
            }
        }

        public static void Enqueue(Action action)
        {
            _instance._actions.Enqueue(action);
        }
    }
}
