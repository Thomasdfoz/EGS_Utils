#if UNITY_EDITOR 
using UnityEditor;
using UnityEditor.Build;
#endif


namespace EGS.Utils 
{
    public class AppBundle : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            NamedBuildTarget buildTargetGroup = NamedBuildTarget.Standalone;
#if !UNITY_EDITOR && UNITY_WEBGL
             buildTargetGroup = NamedBuildTarget.WebGL;
#elif UNITY_ANDROID
            buildTargetGroup = NamedBuildTarget.Android;
#elif UNITY_IOS
            buildTargetGroup = NamedBuildTarget.iOS;
#endif
            PlayerSettings.SetApplicationIdentifier(buildTargetGroup, value.ToString());
#endif
        }
    }
}