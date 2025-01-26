using UnityEngine;


namespace EGS.Utils 
{
    [System.Serializable]
    public struct CustomSceneData
    {
        [HideInInspector] public Color color;
        [HideInInspector] public Texture2D texture;
        public MeshRenderer[] renderers;
    }
}