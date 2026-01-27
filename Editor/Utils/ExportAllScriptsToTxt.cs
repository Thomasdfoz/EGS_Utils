#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ExportAllScriptsToTxt : EditorWindow
{
    private string outputFilePath = "Assets/AllScriptsCombined.txt";
    private string readmeOutputPath = "Assets/README_AI.md";
    private string searchPath = "Assets";
    private string apiKey = ""; // Insira sua chave aqui ou no campo da janela

    private bool includeComments = true;
    private bool skipGenerated = true;
    private bool isProcessing = false;
    private Vector2 scroll;

    private const string PREF_KEY_SEARCH_PATH = "ExportScripts_SearchPath";
    private const string PREF_KEY_API_KEY = "ExportScripts_GeminiKey";

    [MenuItem("Tools/Export All Scripts & Generate README")]
    public static void ShowWindow() => GetWindow<ExportAllScriptsToTxt>("AI Script Exporter").minSize = new Vector2(550, 400);

    private void OnEnable()
    {
        searchPath = EditorPrefs.GetString(PREF_KEY_SEARCH_PATH, "Assets");
        apiKey = EditorPrefs.GetString(PREF_KEY_API_KEY, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export & AI Documentation", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        // --- Configurações de Exportação ---
        EditorGUILayout.BeginVertical("box");
        EditorGUI.BeginChangeCheck();
        searchPath = EditorGUILayout.TextField("Search Folder", searchPath);
        if (EditorGUI.EndChangeCheck()) EditorPrefs.SetString(PREF_KEY_SEARCH_PATH, searchPath);

        outputFilePath = EditorGUILayout.TextField("Output TXT:", outputFilePath);
        readmeOutputPath = EditorGUILayout.TextField("Output README (MD):", readmeOutputPath);
        includeComments = EditorGUILayout.Toggle("Include Comments", includeComments);
        skipGenerated = EditorGUILayout.Toggle("Skip Generated", skipGenerated);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- Configurações da IA ---
        EditorGUILayout.LabelField("AI Settings", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginVertical("box");
        EditorGUI.BeginChangeCheck();
        apiKey = EditorGUILayout.PasswordField("Gemini API Key:", apiKey);
        if (EditorGUI.EndChangeCheck()) EditorPrefs.SetString(PREF_KEY_API_KEY, apiKey);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // --- Botões de Ação ---
        if (isProcessing)
        {
            GUI.enabled = false;
            GUILayout.Button("Processing with AI... Please wait.");
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Step 1: Export Scripts to TXT", GUILayout.Height(30)))
            {
                RunExport();
                EditorUtility.DisplayDialog("Success", "Scripts exported to " + outputFilePath, "OK");
            }

            if (GUILayout.Button("Step 2: Generate AI README from TXT", GUILayout.Height(30)))
            {
                _ = RequestAIRecap(); // Chama o método async
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void RunExport()
    {
        string absoluteOutput = Path.GetFullPath(outputFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutput));

        StringBuilder sb = new StringBuilder();
        string[] files = Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (skipGenerated && IsGenerated(file)) continue;

            string text = File.ReadAllText(file);
            if (!includeComments) text = StripCommentsSimple(text);

            sb.AppendLine($"// --- FILE: {file.Replace("\\", "/")} ---");
            sb.AppendLine(text.Trim());
            sb.AppendLine();
        }

        File.WriteAllText(absoluteOutput, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
    }

    private bool IsGenerated(string path) => path.Contains(".g.") || path.Contains(".Designer.") || path.ToLower().Contains("generated");

    private async Task RequestAIRecap()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            EditorUtility.DisplayDialog("Error", "Please provide a Gemini API Key.", "OK");
            return;
        }

        if (!File.Exists(outputFilePath))
        {
            RunExport();
        }

        isProcessing = true;
        string codeContent = File.ReadAllText(outputFilePath);

        // Prompt agressivo para garantir que a IA não mande conversa furada, apenas o MD
        string prompt = $"Analise os scripts C# abaixo de um projeto Unity e crie um arquivo README.md profissional. " +
                        $"Inclua: Nome do projeto (baseado na pasta), visão geral, principais classes/sistemas e instruções de uso. " +
                        $"Responda APENAS com o código Markdown, sem textos explicativos antes ou depois.\n\nCÓDIGO:\n{codeContent}";

        try
        {
            string response = await SendToGemini(prompt);
            // Limpa possíveis marcações de markdown da resposta da IA
            response = response.Replace("```markdown", "").Replace("```", "").Trim();

            File.WriteAllText(readmeOutputPath, response, Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("AI Success", "README.md generated successfully!", "OK");
        }
        catch (Exception e)
        {
            Debug.LogError("Gemini Error: " + e.Message);
        }
        finally
        {
            isProcessing = false;
        }
    }
    private async Task<string> SendToGemini(string prompt)
    {
        // Em 2026, usamos a v1beta para acessar os modelos 2.0 Flash mais recentes
        // O nome do modelo oficial agora é gemini-2.0-flash
        string modelName = "gemini-2.0-flash";
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

        // Estrutura de JSON para o Gemini 2.0
        string manualJson = "{\"contents\":[{\"parts\":[{\"text\":\"" + JsonHelper.Escape(prompt) + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(manualJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // Se o 404 persistir, o Google pode ter mudado o alias para 'gemini-2.0-flash-exp' ou similar
                string errorDetail = request.downloadHandler.text;
                throw new Exception($"Erro na API Gemini ({request.responseCode}): {errorDetail}");
            }

            var responseData = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

            if (responseData?.candidates != null && responseData.candidates.Length > 0)
            {
                return responseData.candidates[0].content.parts[0].text;
            }

            throw new Exception("A IA não retornou conteúdo válido.");
        }
    }

    private static string StripCommentsSimple(string input)
    {
        input = Regex.Replace(input, @"/\*.*?\*/", "", RegexOptions.Singleline);
        input = Regex.Replace(input, @"//.*?$", "", RegexOptions.Multiline);
        return input;
    }

    // Classes auxiliares para o JSON
    [Serializable] private class GeminiResponse { public Candidate[] candidates; }
    [Serializable] private class Candidate { public Content content; }
    [Serializable] private class Content { public Part[] parts; }
    [Serializable] private class Part { public string text; }

    private static class JsonHelper
    {
        public static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
#endif