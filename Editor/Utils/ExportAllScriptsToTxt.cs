#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class AI_ScriptExporterImporter : EditorWindow
{
    // Export
    private string outputFilePath = "Assets/AllScriptsCombined.txt";
    private string searchPath = "Assets";
    private bool includeComments = true;
    private bool skipGeneratedOnExport = true;

    // Import
    private TextAsset inputCombinedTxt;
    private bool importOverwriteExisting = true;
    private bool importOnlyCs = true;
    private bool importSkipGenerated = true;

    // README / AI (optional)
    private string readmeOutputPath = "Assets/README_AI.md";
    private string apiKey = "";

    private bool isProcessing = false;
    private Vector2 scroll;

    private const string PREF_KEY_SEARCH_PATH = "ExportScripts_SearchPath";
    private const string PREF_KEY_API_KEY = "ExportScripts_GeminiKey";

    // Supports both:
    // 1) // --- FILE: Assets/.../X.cs ---
    // 2) FILE: Assets/.../X.cs   (often surrounded by ===== lines)
    private static readonly Regex FileHeaderRegex = new Regex(
        @"^\s*//\s*---\s*FILE:\s*(.+?)\s*---\s*$" +      // Group 1
        @"|^\s*FILE:\s*(Assets\/.+?)\s*$",               // Group 2
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    [MenuItem("Tools/Export-Import Scripts (TXT)")]
    public static void ShowWindow()
    {
        var w = GetWindow<AI_ScriptExporterImporter>("Script TXT Importer");
        w.minSize = new Vector2(650, 560);
    }

    private void OnEnable()
    {
        searchPath = EditorPrefs.GetString(PREF_KEY_SEARCH_PATH, "Assets");
        apiKey = EditorPrefs.GetString(PREF_KEY_API_KEY, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scripts TXT Export / Import", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawImportSection();

        EditorGUILayout.Space(10);

        DrawExportSection();

        EditorGUILayout.Space(10);

        DrawAISection();

        EditorGUILayout.Space(10);

        DrawButtons();

        EditorGUILayout.EndScrollView();
    }

    // =========================
    // IMPORT
    // =========================
    private void DrawImportSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Step 0: Import (TXT -> Create/Update .cs files)", EditorStyles.miniBoldLabel);

        inputCombinedTxt = (TextAsset)EditorGUILayout.ObjectField("Input Combined TXT", inputCombinedTxt, typeof(TextAsset), false);

        importOverwriteExisting = EditorGUILayout.ToggleLeft("Overwrite existing files (replace/update)", importOverwriteExisting);
        importOnlyCs = EditorGUILayout.ToggleLeft("Only create .cs files", importOnlyCs);
        importSkipGenerated = EditorGUILayout.ToggleLeft("Skip Generated (paths containing .g., .Designer., generated)", importSkipGenerated);

        EditorGUILayout.HelpBox(
            "Supported headers:\n" +
            "1) // --- FILE: Assets/.../File.cs ---\n" +
            "2) FILE: Assets/.../File.cs  (can be surrounded by ===== lines)\n\n" +
            "Safety: only writes under Assets/ and inside the project root.",
            MessageType.Info);

        if (GUILayout.Button("Run Import (Create/Update Files)", GUILayout.Height(32)))
        {
            try
            {
                int created, updated, skipped, errors;
                ImportFromCombinedTxt(inputCombinedTxt, importOverwriteExisting, importOnlyCs, importSkipGenerated,
                    out created, out updated, out skipped, out errors);

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Import Done",
                    $"Created: {created}\nUpdated: {updated}\nSkipped: {skipped}\nErrors: {errors}",
                    "OK");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("Import Error", e.Message, "OK");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private static void ImportFromCombinedTxt(
        TextAsset combinedTxt,
        bool overwriteExisting,
        bool onlyCs,
        bool skipGenerated,
        out int created,
        out int updated,
        out int skipped,
        out int errors)
    {
        created = updated = skipped = errors = 0;

        if (combinedTxt == null)
            throw new Exception("Select an Input Combined TXT (TextAsset) first.");

        string all = combinedTxt.text ?? "";
        if (string.IsNullOrWhiteSpace(all))
            throw new Exception("Input TXT is empty.");

        var matches = FileHeaderRegex.Matches(all);
        if (matches.Count == 0)
            throw new Exception("No file headers found. Expected // --- FILE: ... --- OR FILE: Assets/...");

        string projectRoot = Path.GetFullPath(Application.dataPath + "/..");

        for (int i = 0; i < matches.Count; i++)
        {
            // Group 1 or Group 2 depending on which header matched
            string rawPath = matches[i].Groups[1].Success
                ? matches[i].Groups[1].Value.Trim()
                : matches[i].Groups[2].Value.Trim();

            rawPath = rawPath.Replace("\\", "/");

            int contentStart = matches[i].Index + matches[i].Length;
            int contentEnd = (i + 1 < matches.Count) ? matches[i + 1].Index : all.Length;

            string content = all.Substring(contentStart, contentEnd - contentStart);

            // Clean leading whitespace/newlines
            content = content.TrimStart('\r', '\n');

            // If the exporter used "=====" separators, they might still be in content due to formatting.
            // Remove leading separator lines that are just "=====".
            content = StripLeadingSeparatorLines(content);

            // Normalize and ensure final newline for nicer diffs
            content = NormalizeLineEndings(content).TrimEnd() + "\n";

            try
            {
                // Safety: must be under Assets/
                if (!rawPath.StartsWith("Assets/", StringComparison.Ordinal))
                {
                    skipped++;
                    Debug.LogWarning($"[IMPORT] Skipped (outside Assets/): {rawPath}");
                    continue;
                }

                if (skipGenerated && IsGenerated(rawPath))
                {
                    skipped++;
                    continue;
                }

                if (onlyCs && Path.GetExtension(rawPath).ToLowerInvariant() != ".cs")
                {
                    skipped++;
                    continue;
                }

                string fullPath = Path.GetFullPath(rawPath);

                // Safety: must be inside the project root
                if (!fullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    Debug.LogWarning($"[IMPORT] Skipped (path escapes project): {rawPath}");
                    continue;
                }

                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                bool exists = File.Exists(fullPath);
                if (exists && !overwriteExisting)
                {
                    skipped++;
                    continue;
                }

                File.WriteAllText(fullPath, content, new UTF8Encoding(false));

                if (!exists) created++;
                else updated++;
            }
            catch (Exception e)
            {
                errors++;
                Debug.LogError($"[IMPORT] Error writing '{rawPath}': {e.Message}");
            }
        }
    }

    private static string StripLeadingSeparatorLines(string content)
    {
        // Removes lines at the very start that are only '=' characters (common in your format)
        while (true)
        {
            int lineEnd = content.IndexOf('\n');
            string firstLine = lineEnd >= 0 ? content.Substring(0, lineEnd) : content;

            string trimmed = firstLine.Trim();
            if (trimmed.Length > 0 && IsAllEquals(trimmed))
            {
                // remove this line and continue
                if (lineEnd < 0) return "";
                content = content.Substring(lineEnd + 1).TrimStart('\r', '\n');
                continue;
            }

            // Sometimes there's a blank line right after separators, keep trimming a bit
            if (trimmed.Length == 0)
            {
                if (lineEnd < 0) return "";
                content = content.Substring(lineEnd + 1).TrimStart('\r', '\n');
                continue;
            }

            break;
        }

        return content;
    }

    private static bool IsAllEquals(string s)
    {
        for (int i = 0; i < s.Length; i++)
            if (s[i] != '=') return false;
        return true;
    }

    private static string NormalizeLineEndings(string s)
    {
        s = s.Replace("\r\n", "\n");
        s = s.Replace("\r", "\n");
        return s;
    }

    private static bool IsGenerated(string path)
    {
        string p = path.Replace("\\", "/");
        return p.Contains(".g.") ||
               p.Contains(".Designer.") ||
               p.ToLowerInvariant().Contains("generated");
    }

    // =========================
    // EXPORT
    // =========================
    private void DrawExportSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Export (Files -> TXT)", EditorStyles.miniBoldLabel);

        EditorGUI.BeginChangeCheck();
        searchPath = EditorGUILayout.TextField("Search Folder", searchPath);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PREF_KEY_SEARCH_PATH, searchPath);

        outputFilePath = EditorGUILayout.TextField("Output TXT", outputFilePath);

        includeComments = EditorGUILayout.Toggle("Include Comments", includeComments);
        skipGeneratedOnExport = EditorGUILayout.Toggle("Skip Generated", skipGeneratedOnExport);

        if (GUILayout.Button("Export Scripts to TXT", GUILayout.Height(32)))
        {
            RunExport();
            EditorUtility.DisplayDialog("Success", "Scripts exported to " + outputFilePath, "OK");
        }

        EditorGUILayout.EndVertical();
    }

    private void RunExport()
    {
        string absoluteOutput = Path.GetFullPath(outputFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutput));

        StringBuilder sb = new StringBuilder();
        string[] files = Directory.GetFiles(searchPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (skipGeneratedOnExport && IsGenerated(file)) continue;

            string text = File.ReadAllText(file);
            if (!includeComments) text = StripCommentsSimple(text);

            sb.AppendLine($"// --- FILE: {file.Replace("\\", "/")} ---");
            sb.AppendLine(text.TrimEnd());
            sb.AppendLine();
        }

        File.WriteAllText(absoluteOutput, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
    }

    private static string StripCommentsSimple(string input)
    {
        input = Regex.Replace(input, @"/\*.*?\*/", "", RegexOptions.Singleline);
        input = Regex.Replace(input, @"//.*?$", "", RegexOptions.Multiline);
        return input;
    }

    // =========================
    // AI README (optional, same as your idea)
    // =========================
    private void DrawAISection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("AI README (optional)", EditorStyles.miniBoldLabel);

        readmeOutputPath = EditorGUILayout.TextField("Output README (MD)", readmeOutputPath);

        EditorGUI.BeginChangeCheck();
        apiKey = EditorGUILayout.PasswordField("Gemini API Key", apiKey);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PREF_KEY_API_KEY, apiKey);

        EditorGUILayout.EndVertical();
    }

    private void DrawButtons()
    {
        if (isProcessing)
        {
            GUI.enabled = false;
            GUILayout.Button("Processing with AI... Please wait.", GUILayout.Height(30));
            GUI.enabled = true;
            return;
        }

        if (GUILayout.Button("Generate AI README from TXT (uses Output TXT path)", GUILayout.Height(32)))
        {
            _ = RequestAIRecap();
        }
    }

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

        string prompt =
            "Analise os scripts C# abaixo de um projeto Unity e crie um arquivo README.md profissional. " +
            "Inclua: Nome do projeto (baseado na pasta), visão geral, principais classes/sistemas e instruções de uso. " +
            "Responda APENAS com o código Markdown, sem textos explicativos antes ou depois.\n\nCÓDIGO:\n" + codeContent;

        try
        {
            string response = await SendToGemini(prompt);
            response = response.Replace("```markdown", "").Replace("```", "").Trim();

            File.WriteAllText(readmeOutputPath, response, Encoding.UTF8);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("AI Success", "README.md generated successfully!", "OK");
        }
        catch (Exception e)
        {
            Debug.LogError("Gemini Error: " + e.Message);
            EditorUtility.DisplayDialog("AI Error", e.Message, "OK");
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async Task<string> SendToGemini(string prompt)
    {
        // Keep as you had: v1beta + gemini-2.0-flash
        string modelName = "gemini-2.0-flash";
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";

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
                string errorDetail = request.downloadHandler.text;
                throw new Exception($"Erro na API Gemini ({request.responseCode}): {errorDetail}");
            }

            var responseData = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

            if (responseData?.candidates != null && responseData.candidates.Length > 0)
                return responseData.candidates[0].content.parts[0].text;

            throw new Exception("A IA não retornou conteúdo válido.");
        }
    }

    [Serializable] private class GeminiResponse { public Candidate[] candidates; }
    [Serializable] private class Candidate { public Content content; }
    [Serializable] private class Content { public Part[] parts; }
    [Serializable] private class Part { public string text; }

    private static class JsonHelper
    {
        public static string Escape(string s)
            => s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
    }
}
#endif