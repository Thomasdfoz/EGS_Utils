#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EGS.Utils 
{
    public class CompanyName : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            PlayerSettings.companyName = value.ToString();
#endif
        }
    }
}
