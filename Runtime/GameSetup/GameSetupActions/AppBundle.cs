#if UNITY_EDITOR 
using UnityEditor;
#endif


namespace EGS.Utils 
{
    public class AppBundle : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
#if !UNITY_EDITOR && UNITY_WEBGL
             buildTargetGroup = BuildTargetGroup.WebGL;
#elif UNITY_ANDROID
            buildTargetGroup = BuildTargetGroup.Android;
#elif UNITY_IOS
            buildTargetGroup = BuildTargetGroup.iOS;
#endif
            PlayerSettings.SetApplicationIdentifier(buildTargetGroup, value.ToString());
#endif
        }
    }
}