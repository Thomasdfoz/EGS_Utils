using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using EGS.Utils;

namespace EGS.Utils.Editor 
{
    [CustomEditor(typeof(Readme))]
    public class ReadmeEditor : UnityEditor.Editor
    {
        public static Queue<string> m_QueueLines = new Queue<string>();
        public static Dictionary<string, Action<Readme.Line>> MarkDownDictionary = new Dictionary<string, Action<Readme.Line>>();

        private static void ReaderReadmeArchieve(Readme readme)
        {
            CreateDictionary();

            if (readme == null)
                return;

            readme.Header = string.Empty;
            readme.Lines.Clear();

            string lPath = Application.dataPath + "/../" + "/README.md";

            try
            {
                StreamReader lStreamReader = new StreamReader(lPath);

                string lLinha = string.Empty;
                string temp = string.Empty;
                while ((lLinha = lStreamReader.ReadLine()) != null)
                {
                    temp = ClearHtmlTags(lLinha);

                    if (temp != string.Empty && lLinha != string.Empty)
                    {
                        m_QueueLines.Enqueue(lLinha);
                    }
                }

                lStreamReader.Close();
            }
            catch (FileNotFoundException ex)
            {

                Debug.LogWarning($"Arquivo Readme n�o existe, Crie um arquivo README.md na raiz do repositorio! " + ex.Message);

            }

            readme.Header = ClearHtmlTags(m_QueueLines.Dequeue());

            while (m_QueueLines.Count() > 0)
            {
                UpdateLine(m_QueueLines.Dequeue(), readme);
            }

        }

        public static void UpdateLine(string line, Readme readme)
        {
            Readme.Line newline = new Readme.Line();

            string lTemp = line;

            lTemp = lTemp.Replace("</b>", string.Empty);

            if (lTemp.Contains("#### "))
            {
                lTemp = lTemp.Replace("#### ", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["####"](newline);

            }
            if (lTemp.Contains("### "))
            {
                lTemp = lTemp.Replace("### ", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["###"](newline);
            }
            if (lTemp.Contains("## "))
            {
                lTemp = lTemp.Replace("## ", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["##"](newline);
            }
            if (lTemp.Contains("# "))
            {
                lTemp = lTemp.Replace("# ", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["##"](newline);
            }
            if (lTemp.Contains("<br>"))
            {

                lTemp = lTemp.Replace("<br>", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["<br>"](newline);
            }
            if (lTemp.Contains("<b>"))
            {
                lTemp = lTemp.Replace("<b>", string.Empty);
                newline.Text = lTemp;
                MarkDownDictionary["<b>"](newline);
            }
            newline.Text = lTemp;


            readme.Lines.Add(newline);
        }

        public static string ClearHtmlTags(string text)
        {
            string lTemp = text;
            lTemp = lTemp.Replace("#### ", string.Empty);
            lTemp = lTemp.Replace("### ", string.Empty);
            lTemp = lTemp.Replace("## ", string.Empty);
            lTemp = lTemp.Replace("# ", string.Empty);
            lTemp = lTemp.Replace("<br>", string.Empty);
            lTemp = lTemp.Replace("<b>", string.Empty);
            lTemp = lTemp.Replace("<b>", string.Empty);
            return lTemp;
        }

        public static void CreateDictionary()
        {
            if (MarkDownDictionary.ContainsKey("#"))
            {
                return;
            }
            MarkDownDictionary.Add("#", (line) =>
            {
                line.FontSize = 25;
            });
            MarkDownDictionary.Add("##", (line) =>
            {
                ColorUtility.TryParseHtmlString("#99d3e9", out line.Color);
                line.FontSize = 20;

            });
            MarkDownDictionary.Add("###", (line) =>
            {
                ColorUtility.TryParseHtmlString("#dedede", out line.Color);
                line.FontSize = 18;

            });
            MarkDownDictionary.Add("####", (line) =>
            {
                ColorUtility.TryParseHtmlString("#dedede", out line.Color);
                line.FontSize = 16;
            });
            MarkDownDictionary.Add("<br>", (line) =>
            {
                line.IsBreakLine = true;
            });
            MarkDownDictionary.Add("<b>", (line) =>
            {
                line.IsBold = true;
            });

        }

        [InitializeOnLoadMethod]
        static void InitiGUI()
        {
            EditorApplication.delayCall += SelectReadme;
        }

        static void SelectReadme()
        {
            EditorApplication.delayCall -= SelectReadme;
            Readme lReadmeObject = Resources.Load<Readme>("Readme");

            if (lReadmeObject == null)
                return;

            if (lReadmeObject.IsNotOpen)
                return;

            ReaderReadmeArchieve(lReadmeObject);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = lReadmeObject;

        }
        protected override void OnHeaderGUI()
        {
            Readme lReadme = (Readme)target;

            GUILayout.BeginHorizontal();

            var lStyle = GUI.skin.GetStyle("label");
            lStyle.fontSize = 25;
            lStyle.fontStyle = FontStyle.Bold;
            Color lNewColor;
            ColorUtility.TryParseHtmlString("#33bcee", out lNewColor);
            lStyle.normal.textColor = lNewColor;
            GUILayout.Label(lReadme.Header, lStyle, GUILayout.Height(60));

            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {

            Readme lReadme = (Readme)target;

            for (int i = 0; i < lReadme.Lines.Count(); i++)
            {

                var lLine = lReadme.Lines[i];

                var lStyle = GUI.skin.GetStyle("label");

                lStyle.fontSize = lLine.FontSize;

                lStyle.fontStyle = lLine.IsBold ? FontStyle.Bold : FontStyle.Normal;

                lStyle.normal.textColor = Color.white;

                lStyle.normal.textColor = lLine.Color;

                lStyle.wordWrap = true;

                GUILayoutOption[] lOptions = null;

                if (lLine.FontSize == 14 && lLine.IsBold)
                {
                    GUILayout.Space(2);
                }

                if (i > 0 && lLine.FontSize == 20)
                {
                    GUILayout.Space(30);
                }

                if (lLine.FontSize >= 20)
                    lOptions = new GUILayoutOption[] { GUILayout.Height(Mathf.Ceil(lLine.Text.Count() / 50f) * 25f), GUILayout.Width(600) };
                else if (lLine.FontSize >= 18)
                    lOptions = new GUILayoutOption[] { GUILayout.Height(Mathf.Ceil(lLine.Text.Count() / 60f) * 25f), GUILayout.Width(600) };
                else if (lLine.FontSize >= 16)
                    lOptions = new GUILayoutOption[] { GUILayout.Height(Mathf.Ceil(lLine.Text.Count() / 70f) * 25f), GUILayout.Width(600) };
                else if (lLine.FontSize <= 14 && lLine.IsBold)
                    lOptions = new GUILayoutOption[] { GUILayout.Height(Mathf.Ceil(lLine.Text.Count() / 85f) * 20f), GUILayout.Width(700) };
                else if (lLine.FontSize <= 14)
                    lOptions = new GUILayoutOption[] { GUILayout.Height(Mathf.Ceil(lLine.Text.Count() / 85f) * 16f), GUILayout.Width(600) };


                GUILayout.Label(lLine.Text, lStyle, lOptions);

            }

            var lGlobalStyle = GUI.skin.GetStyle("label");

            GUILayout.Space(20);

            lGlobalStyle.fontSize = 18;
            lGlobalStyle.richText = true;

            EditorGUILayout.SelectableLabel("<a  href=\"https://bookstack.vrglass.com/login\">Documentation link</a>", lGlobalStyle);

            GUILayout.Space(20);

            lGlobalStyle.fontStyle = FontStyle.Normal;
            lGlobalStyle.fontSize = 12;
            lGlobalStyle.normal.textColor = Color.white;

            lReadme.IsNotOpen = GUILayout.Toggle(lReadme.IsNotOpen, "N�o aparecer novamente!");
            lReadme.hideFlags = HideFlags.DontUnloadUnusedAsset;

            if (GUILayout.Button("Reload"))
            {
                ReaderReadmeArchieve(lReadme);
            }

        }
    }
}