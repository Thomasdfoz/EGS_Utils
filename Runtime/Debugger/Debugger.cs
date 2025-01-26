using UnityEngine;

namespace EGS.Utils 
{
    public static class Debugger
    {
        public enum LogColors
        {
            White,
            Cyan,
            Orange,
            Yellow,
            Blue,
            Magenta,
            Red,
            Black,
            Grey,
            Brown,
            Green,
            Lightblue,
            Lime,
            Olive,
            Purple,
            Silver,
            Teal,
        }


        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void Log(string text)
        {
#if UNITY_EDITOR || DEBUGGER_IN_BUILD
            Debug.Log($"<b><color={LogColors.White.ToString().ToLower()}>{text}</color></b>");
#elif UNITY_WEBGL
                			Application.ExternalCall("console.log", text);
#endif
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void Log(LogColors color, string text)
        {
#if UNITY_EDITOR || DEBUGGER_IN_BUILD
            Debug.Log($"<b><color={color.ToString().ToLower()}>{text}</color></b>");
#elif UNITY_WEBGL
                            Application.ExternalCall("console.log", text);
#endif
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void Log(LogColors color, string tag, string text)
        {
#if UNITY_EDITOR || DEBUGGER_IN_BUILD
            Debug.Log($"<b><color={color.ToString().ToLower()}>[{tag}] {text}</color></b>");
#elif UNITY_WEBGL
                			Application.ExternalCall("console.log", text);
#endif
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void LogError(string text)
        {
            Debug.LogError($"<b><color={LogColors.Red.ToString().ToLower()}>{text}</color></b>");
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void LogError(string tag, string text)
        {
            Debug.LogError($"<b><color={LogColors.Red.ToString().ToLower()}>[{tag}] {text}</color></b>");
        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void LogWarning(string text)
        {
            Debug.LogWarning($"<b><color={LogColors.Yellow.ToString().ToLower()}>{text}</color></b>");

        }

        [System.Diagnostics.Conditional("ENABLE_DEBUGGER")]
        public static void LogWarning(string tag, string text)
        {
            Debug.LogWarning($"<b><color={LogColors.Yellow.ToString().ToLower()}>[{tag}] {text}</color></b>");
        }

    }
}