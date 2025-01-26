using System.Net;
using System.IO;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using System;


namespace EGS.Utils 
{
    public class AppIconUrl : GameSetupAction
    {
        public override void ExecuteAction(object value)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(value.ToString()))
            {
                if (!string.IsNullOrEmpty(value.ToString()) && Uri.IsWellFormedUriString(value.ToString(), UriKind.Absolute))
                {

                    UpdateLocalIcon(value.ToString());
#if UNITY_ANDROID
                    BuildTargetGroup btg = BuildTargetGroup.Android;
                    UpdateAndroidIcons(btg);
#elif UNITY_IOS
                    BuildTargetGroup btg =  BuildTargetGroup.iOS;
                    UpdateAndroidIcons(btg);
#endif
                }
            }
#endif

        }

#if UNITY_EDITOR
        private void UpdateLocalIcon(string url)
        {
            WebClient lrequest = new WebClient();
            byte[] ldata = lrequest.DownloadData(url);

            string lIconPath = Application.dataPath + "/Resources/";
            File.WriteAllBytes($"{lIconPath}/icon-whitelabel.png", ldata);

            lrequest.Dispose();
            AssetDatabase.Refresh();

        }

        private void UpdateAndroidIcons(BuildTargetGroup buildTargetGroup)
        {
            PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKindsForPlatform(buildTargetGroup);

            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(buildTargetGroup, kinds[0]);

            Texture2D iconFG = (Texture2D)Resources.Load("icon-whitelabel");
            Texture2D iconBG = (Texture2D)Resources.Load("icon-whitelabel");

            for (int i = 0; i < icons.Length; i++)
            {
                int w = icons[i].width;
                int h = icons[i].height;

                // Resize to icon dimensions
                iconFG = TextureScaler(iconFG, w, h, FilterMode.Bilinear);
                iconBG = TextureScaler(iconBG, w, h, FilterMode.Bilinear);

                // Save the textures
                AssetDatabase.CreateAsset(iconFG, "Assets/Resources/Icons/Android/icon" + w.ToString() + "x" + h.ToString() + "_fg.mat");
                AssetDatabase.CreateAsset(iconBG, "Assets/Resources/Icons/Android/icon" + w.ToString() + "x" + h.ToString() + "_bg.mat");

                // Assign the textures
                icons[i].SetTexture((Texture2D)Resources.Load("Icons/Android/icon" + w.ToString() + "x" + h.ToString() + "_fg"), 0);
                if (icons[i].layerCount > 1)
                    icons[i].SetTexture((Texture2D)Resources.Load("Icons/Android/icon" + w.ToString() + "x" + h.ToString() + "_bg"), 1);
            }
            PlayerSettings.SetPlatformIcons(buildTargetGroup, kinds[0], icons);
        }
        private Texture2D TextureScaler(Texture2D source, int targetWidth, int targetHeight, FilterMode filterMode)
        {
            Rect texRect = new Rect(0, 0, targetWidth, targetHeight);
            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0);
            rt.filterMode = filterMode;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            Texture2D scaledTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            scaledTexture.filterMode = filterMode;
            scaledTexture.ReadPixels(texRect, 0, 0);
            scaledTexture.Apply();

            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            return scaledTexture;
        }
#endif
    }
}
