using UnityEngine;


namespace EGS.Utils 
{
    public class MaterialFixer
    {
        public static void FixMaterials()
        {
            MeshRenderer[] meshRenderers = GameObject.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            SkinnedMeshRenderer[] skinnedMeshRenderers = GameObject.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                for (int j = 0; j < meshRenderers[i].materials.Length; j++)
                {
                    meshRenderers[i].materials[j].shader = Shader.Find(meshRenderers[i].material.shader.name);
                }
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                for (int j = 0; j < skinnedMeshRenderers[i].materials.Length; j++)
                {
                    skinnedMeshRenderers[i].materials[j].shader = Shader.Find(skinnedMeshRenderers[i].material.shader.name);
                }
            }
            RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
        }
    }

}