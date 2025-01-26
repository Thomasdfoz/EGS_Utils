/*#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EGS.Utils.Editor 
{
[CustomEditor(typeof(GameSetup))]
public class GameSetupEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        GUILayout.Space(10);

        GameSetup settings = (GameSetup)target;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedHeight = 30,
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        EditorGUILayout.BeginHorizontal();

        // Bot�o de Validate com �cone padr�o do Unity
        if (GUILayout.Button(new GUIContent(" Execute Validate", EditorGUIUtility.IconContent("d_SaveAs").image), buttonStyle))
        {
            settings.Validate();
        }

        GUILayout.Space(10); // Espa�o entre os bot�es

        // Bot�o de Listar Todos com �cone padr�o do Unity
        if (GUILayout.Button(new GUIContent(" List All", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image), buttonStyle))
        {
            settings.ListAllEnvironmentVariables();
        }

        GUILayout.Space(10); // Espa�o entre os bot�es

        // Bot�o de Limpar Tudo com �cone padr�o do Unity
        if (GUILayout.Button(new GUIContent(" Clear All", EditorGUIUtility.IconContent("d_TreeEditor.Trash").image), buttonStyle))
        {
            settings.ClearEnvironmentVariables();
        }

        EditorGUILayout.EndHorizontal();
    }
}
}
#endif
*/