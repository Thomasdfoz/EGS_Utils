using System.IO;
using UnityEditor;
using UnityEngine;

namespace EGS.Utils.Editor
{
    public class NamespaceUpdater : EditorWindow
    {
        private string namespaceName = "";
        private string folderPath = "";

        [MenuItem("Tools/Namespace Updater")]
        public static void ShowWindow()
        {
            GetWindow<NamespaceUpdater>("Namespace Updater");
        }

        private void OnGUI()
        {
            // Título principal estilizado
            GUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.green }
            };
            GUILayout.Label("Namespace Updater", titleStyle);

            GUILayout.Space(10);

            // Campo para o namespace com estilo
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label) { fontSize = 12, fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField("Namespace:", labelStyle);
            namespaceName = EditorGUILayout.TextField(namespaceName);

            GUILayout.Space(10);

            // Botão para selecionar a pasta
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white, background = Texture2D.grayTexture },
                hover = { textColor = Color.green }
            };

            if (GUILayout.Button("Selecionar Pasta", buttonStyle))
            {
                folderPath = EditorUtility.OpenFolderPanel("Selecionar Pasta", Application.dataPath, "");
            }

            GUILayout.Space(5);

            // Mostrar a pasta selecionada
            if (!string.IsNullOrEmpty(folderPath))
            {
                GUIStyle pathStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontSize = 11,
                    normal = { textColor = Color.green }
                };
                GUILayout.Label($"Pasta Selecionada: {folderPath}", pathStyle);
            }

            GUILayout.Space(10);

            // Botão para adicionar namespace nos scripts
            if (GUILayout.Button("Adicionar Namespace nos Scripts", buttonStyle))
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    Debug.LogError("Por favor, selecione uma pasta primeiro!");
                }
                else if (string.IsNullOrEmpty(namespaceName))
                {
                    Debug.LogError("Por favor, insira um namespace válido!");
                }
                else
                {
                    AddNamespaceToScripts(folderPath, namespaceName);
                }
            }

            GUILayout.Space(10);

            // Rodapé estilizado
            GUIStyle footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 10,
                fontStyle = FontStyle.Italic
            };
            GUILayout.Label("Desenvolvido com ❤️ por [Thomas Barros]", footerStyle);
        }

        private void AddNamespaceToScripts(string path, string namespaceName)
        {
            string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);

                // Atualiza o namespace preservando o c�digo
                string updatedContent = UpdateNamespace(content, namespaceName);

                // Salva o arquivo com o novo conte�do
                File.WriteAllText(file, updatedContent);
                Debug.Log($"Namespace atualizado no arquivo: {file}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Processo conclu�do!");
        }

        private string UpdateNamespace(string content, string newNamespace)
        {
            // Localiza o �ndice do namespace existente
            int namespaceStart = content.IndexOf("namespace");
            if (namespaceStart >= 0)
            {
                // Encontra o in�cio do namespace e o �ndice da primeira abertura de chave "{"
                int namespaceEnd = content.IndexOf("{", namespaceStart);
                if (namespaceEnd >= 0)
                {
                    // Substitui apenas o namespace entre "namespace" e a primeira "{"
                    string beforeNamespace = content.Substring(0, namespaceStart); // Tudo antes do namespace
                    string afterNamespace = content.Substring(namespaceEnd);       // Tudo a partir da primeira "{"

                    // Retorna o conte�do atualizado com o novo namespace
                    return $"{beforeNamespace}namespace {newNamespace} \n{afterNamespace}";
                }
            }
            // Se n�o houver namespace, insere um novo ap�s os "using"
            int firstNonUsingIndex = FindFirstNonUsingIndex(content);
            string usings = content.Substring(0, firstNonUsingIndex).Trim();
            string code = content.Substring(firstNonUsingIndex).Trim();

            return $"{usings}\n\nnamespace {newNamespace}\n{{\n{IndentContent(code)}\n}}";
        }

        private int FindFirstNonUsingIndex(string content)
        {
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].TrimStart().StartsWith("using") && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    return content.IndexOf(lines[i]);
                }
            }
            return content.Length; // Caso n�o encontre, retorna o final do conte�do
        }

        private string IndentContent(string content)
        {
            // Adiciona indenta��o de 4 espa�os em cada linha
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "    " + lines[i];
            }
            return string.Join("\n", lines);
        }
    }
}