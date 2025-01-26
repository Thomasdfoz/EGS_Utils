using System;
using System.Collections.Generic;

namespace EGS.Utils 
{
    public static class EventManager
    {
        private static Dictionary<string, Action<object>> m_eventParams = new Dictionary<string, Action<object>>();
        private static Dictionary<string, Action> m_eventNotParams = new Dictionary<string, Action>();

        private static Dictionary<string, object> m_eventsTriggereds = new Dictionary<string, object>();

        public static void StartListening(string eventName, Action<object> listener, bool includedBuffered = false)
        {
            if (includedBuffered && m_eventsTriggereds.ContainsKey(eventName))
            {
                listener.Invoke(m_eventsTriggereds[eventName]);
            }

            if (m_eventParams.TryGetValue(eventName, out Action<object> thisEvent))
            {
                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                m_eventParams[eventName] = thisEvent;
            }
            else
            {
                //Add event to the Dictionary for the first time
                thisEvent += listener;
                m_eventParams.Add(eventName, thisEvent);
            }
        }

        public static void StartListening(string eventName, Action listener, bool includedBuffered = false)
        {
            if (includedBuffered && m_eventsTriggereds.ContainsKey(eventName))
            {
                listener.Invoke();
            }

            if (m_eventNotParams.TryGetValue(eventName, out Action thisEvent))
            {
                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                m_eventNotParams[eventName] = thisEvent;
            }
            else
            {
                //Add event to the Dictionary for the first time
                thisEvent += listener;
                m_eventNotParams.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<object> listener)
        {
            if (m_eventParams.TryGetValue(eventName, out Action<object> thisEvent))
            {
                //Remove event from the existing one
                thisEvent -= listener;

                //Update the Dictionary
                m_eventParams[eventName] = thisEvent;
            }
        }

        public static void StopListening(string eventName)
        {
            if (m_eventNotParams.ContainsKey(eventName))
            {
                // Set the Value of the Key to null, effectively removing all listeners
                m_eventNotParams[eventName] = null;
            }
            if (m_eventParams.ContainsKey(eventName))
            {
                // Set the Value of the Key to null, effectively removing all listeners
                m_eventParams[eventName] = null;
            }
        }

        public static void StopListening(string eventName, Action listener)
        {
            if (m_eventNotParams.TryGetValue(eventName, out Action thisEvent))
            {
                //Remove event from the existing one
                thisEvent -= listener;

                //Update the Dictionary
                m_eventNotParams[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, object eventParam, bool oneShot = false)
        {
            if (m_eventParams.TryGetValue(eventName, out Action<object> thisEvent))
            {
                if (thisEvent != null)
                {
                    thisEvent.Invoke(eventParam);

                    if(oneShot)
                        StopListening(eventName, thisEvent);
                    
                }
            }

            m_eventsTriggereds[eventName] = eventParam;
        }

        public static void TriggerEvent(string eventName, bool oneShot = false)
        {
            if (m_eventNotParams.TryGetValue(eventName, out Action thisEvent))
            {
                if (thisEvent != null)
                {
                    thisEvent.Invoke();

                    if(oneShot)
                        StopListening(eventName, thisEvent);
                }
            }

            m_eventsTriggereds[eventName] = null;
        }
    }
}
