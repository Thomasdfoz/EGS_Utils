using System;
using System.Collections.Generic;
using UnityEngine;

namespace EGS.Utils.Editor 
{
    [CreateAssetMenu(fileName = "Readme", menuName = "Create Readme")]
    public class Readme : ScriptableObject
    {
        public string Header;
        public bool IsNotOpen;
        public List<Line> Lines = new List<Line>();

        [Serializable]
        public class Line
        {
            public string Text = string.Empty;
            public int FontSize = 14;
            public bool IsBold = false;
            public bool IsItalic = false;
            public Color Color = Color.white;
            public bool IsBreakLine = false;
        }
    }
}
