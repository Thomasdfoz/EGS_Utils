using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EGS.Utils.Editor 
{
    public class PreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            GameSetup lGameSetup = Resources.Load<GameSetup>(nameof(GameSetup));
            lGameSetup.InitializeVariables();
            ApplicationSetup(lGameSetup);
        }

        private void ApplicationSetup(GameSetup gameSetup)
        {
            try
            {
                for (int i = 0; i < gameSetup.variables.Length; i++)
                {
                    string lGetEnvironmentVariable = Environment.GetEnvironmentVariable(gameSetup.variables[i].Key);

                    if (!string.IsNullOrEmpty(lGetEnvironmentVariable))
                        gameSetup.variables[i].SetValue(lGetEnvironmentVariable);

                    try
                    {
                        if (!string.IsNullOrEmpty(gameSetup.variables[i].ActionTypeName))
                            gameSetup.variables[i].SetupAction.ExecuteAction(gameSetup.variables[i].GetValue());
                    }
                    catch (Exception error)
                    {
                        Debug.LogError($"Error when try to update icon with error: {error}");
                        throw new BuildFailedException("Houve um erro durante a pr�-compila��o. A compila��o ser� interrompida.");
                    }
                }

                EditorUtility.SetDirty(gameSetup);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception error)
            {
                Debug.LogError($"Error when try to update application info with error: {error}");
            }
        }
    }
}