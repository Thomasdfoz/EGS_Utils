#if UNITY_EDITOR
using UnityEditor;
#endif


namespace EGS.Utils 
{
    public class AppName : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            PlayerSettings.productName = value.ToString();
#endif
        }
    }
}