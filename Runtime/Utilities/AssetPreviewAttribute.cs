using System;
using UnityEngine;


namespace EGS.Utils 
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AssetPreviewAttribute : PropertyAttribute
    {
        public AssetPreviewAttribute() { }
    }
}