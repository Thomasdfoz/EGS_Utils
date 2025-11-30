// Place this file under Assets/Editor/ExportAllScriptsToTxt.cs
// Usage: Unity menu -> Tools -> Export All Scripts To Single TXT
#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExportAllScriptsToTxt : EditorWindow
{
    private string outputFilePath = "Assets/AllScriptsCombined.txt";

    // Pasta raiz onde o script vai começar a procurar
    private string searchPath = "Assets";

    private bool includeComments = true;
    private bool skipGenerated = true;
    private Vector2 scroll;

    // Chave única para salvar a preferência no registro do Editor
    private const string PREF_KEY_SEARCH_PATH = "ExportScripts_SearchPath";

    [MenuItem("Tools/Export All Scripts To Single TXT")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<ExportAllScriptsToTxt>("Export Scripts");
        wnd.minSize = new Vector2(520, 250);
    }

    // --- NOVO: Carrega o valor salvo quando a janela abre ---
    private void OnEnable()
    {
        // Tenta pegar o valor salvo. Se não existir, usa "Assets" como padrão.
        searchPath = EditorPrefs.GetString(PREF_KEY_SEARCH_PATH, "Assets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export .cs files to a single .txt", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");

        // --- ALTERADO: Detecta mudanças para salvar automaticamente ---
        EditorGUI.BeginChangeCheck(); // Começa a vigiar mudanças

        searchPath = EditorGUILayout.TextField(new GUIContent("Search Folder", "A pasta raiz onde os scripts serão buscados (ex: Assets/_Game)"), searchPath);

        if (EditorGUI.EndChangeCheck()) // Se mudou algo no campo acima...
        {
            // ...salva imediatamente nas preferências do Editor
            EditorPrefs.SetString(PREF_KEY_SEARCH_PATH, searchPath);
        }

        outputFilePath = EditorGUILayout.TextField("Output file:", outputFilePath);
        includeComments = EditorGUILayout.Toggle("Include comments", includeComments);
        skipGenerated = EditorGUILayout.Toggle("Skip generated files", skipGenerated);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Validação visual
        if (!Directory.Exists(searchPath))
        {
            EditorGUILayout.HelpBox($"A pasta '{searchPath}' não foi encontrada!", MessageType.Error);
            GUI.enabled = false;
        }

        if (GUILayout.Button("Run Export"))
        {
            try
            {
                RunExport();
                EditorUtility.DisplayDialog("Export Complete", $"Generated at:\n{outputFilePath}\n\nScanned folder:\n{searchPath}", "OK");
            }
            catch (Exception ex)
            {
                Debug.LogError("Export failed: " + ex);
                EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
            }
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(80));
        EditorGUILayout.HelpBox(
            $"This tool exports .cs files starting from '{searchPath}'.\n" +
            "The output file will always be created inside Assets/.\n" +
            "Each class/namespace block is separated by a blank line.",
            MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    private void RunExport()
    {
        var absoluteOutput = Path.GetFullPath(outputFilePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absoluteOutput));

        var sb = new StringBuilder();
        string targetFolder = string.IsNullOrEmpty(searchPath) ? "Assets" : searchPath;

        var files = Directory.GetFiles(targetFolder, "*.cs", SearchOption.AllDirectories);

        var units = new List<string>();

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);

            if (skipGenerated)
            {
                if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
                    fileName.IndexOf("generated", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
            }

            string text = File.ReadAllText(file, Encoding.UTF8);
            if (!includeComments)
                text = StripCommentsSimple(text);

            var extracted = ExtractTopLevelUnits(text);

            string displayPath = file.Replace("\\", "/");

            if (extracted.Count == 0)
            {
                extracted.Add($"// --- FILE: {displayPath} ---\n" + text.Trim());
            }
            else
            {
                for (int i = 0; i < extracted.Count; i++)
                    extracted[i] = $"// --- FROM: {displayPath} ---\n" + extracted[i].Trim();
            }

            units.AddRange(extracted);
        }

        for (int i = 0; i < units.Count; i++)
        {
            sb.AppendLine(units[i]);
            if (i < units.Count - 1)
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

    private static List<string> ExtractTopLevelUnits(string text)
    {
        var units = new List<string>();
        int idx = 0;
        var work = text;

        var nsPattern = new Regex(@"\bnamespace\s+[A-Za-z0-9_\.]+\s*{", RegexOptions.Compiled);
        var nsMatches = nsPattern.Matches(work);
        var consumed = new bool[work.Length];

        foreach (Match m in nsMatches)
        {
            int start = m.Index;
            int braceStart = work.IndexOf('{', m.Index + m.Length - 1);
            if (braceStart < 0) continue;
            int end = FindMatchingBrace(work, braceStart);
            if (end < 0) continue;

            string block = work.Substring(start, end - start + 1);
            units.Add(block.Trim());
            for (int k = start; k <= end && k < consumed.Length; k++) consumed[k] = true;
        }

        var typePattern = new Regex(@"\b(class|struct|interface|enum)\s+[A-Za-z0-9_\<\>]+", RegexOptions.Compiled);
        var typeMatches = typePattern.Matches(work);

        foreach (Match m in typeMatches)
        {
            int start = m.Index;
            if (start < consumed.Length && consumed[start]) continue;

            int braceStart = work.IndexOf('{', m.Index + m.Length);
            if (braceStart < 0) continue;
            int end = FindMatchingBrace(work, braceStart);
            if (end < 0) continue;

            string block = work.Substring(start, end - start + 1);
            units.Add(block.Trim());
            for (int k = start; k <= end && k < consumed.Length; k++) consumed[k] = true;
        }

        return units;
    }

    private static int FindMatchingBrace(string text, int openPos)
    {
        int depth = 0;
        for (int i = openPos; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }
}
#endif