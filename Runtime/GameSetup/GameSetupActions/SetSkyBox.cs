using System.Net;
using System.IO;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;


namespace EGS.Utils 
{
    public class SetSkyBox : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            WebClient lrequest = new WebClient();
            byte[] ldata = lrequest.DownloadData(value.ToString());

            string lSkyboxPath = Application.dataPath + "/Resources";
            File.WriteAllBytes($"{lSkyboxPath}/skybox.png", ldata);

            lrequest.Dispose();
            AssetDatabase.Refresh();

            Texture2D skyTexture = (Texture2D)Resources.Load("skybox");

            if (skyTexture != null)
            {
                // Cria um material de Skybox din�mico
                Material skyboxMaterial = new Material(Shader.Find("Skybox/Panoramic"));
                skyboxMaterial.SetTexture("_MainTex", skyTexture);

                // Salva o material como um Asset no projeto
                string materialPath = "Assets/Resources/SkyboxMaterial.mat";
                AssetDatabase.CreateAsset(skyboxMaterial, materialPath);
                AssetDatabase.SaveAssets();

                // Aplica o material de Skybox
                RenderSettings.skybox = skyboxMaterial;
            }
            else
            {
                Debug.LogError("Failed to load skybox texture.");
            }
#endif
        }
    }
}
