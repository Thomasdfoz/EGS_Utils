#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;


namespace EGS.Utils.Editor 
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Iniciar a propriedade
            EditorGUI.BeginProperty(position, label, property);

            // Obter a posi��o do campo
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Ajustar a largura do campo para o asset
            var assetRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Desenhar o campo para o asset
            EditorGUI.PropertyField(assetRect, property, GUIContent.none);

            // Desenhar a pr�-visualiza��o do asset, somente se o campo n�o for null
            if (property.objectReferenceValue != null)
            {
                Type propertyType = property.objectReferenceValue.GetType();

                // Verificar se o tipo do asset � suportado (GameObject ou Sprite)
                if (propertyType == typeof(GameObject) || propertyType == typeof(Sprite))
                {
                    Texture assetPreview = AssetPreview.GetAssetPreview(property.objectReferenceValue);
                    if (assetPreview != null)
                    {
                        float aspectRatio = assetPreview.width / (float)assetPreview.height;
                        float previewHeight = 100;
                        float previewWidth = previewHeight * aspectRatio;

                        Rect previewRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, previewWidth, previewHeight);
                        GUI.DrawTexture(previewRect, assetPreview, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        EditorGUI.HelpBox(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight),
                            "Unable to generate a preview for this asset", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUI.HelpBox(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight),
                     "Property type not supported for preview", MessageType.Warning);
                }
            }

            // Finalizar a propriedade
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Verificar se o campo n�o � null antes de adicionar espa�o extra para o preview
            if (property.objectReferenceValue != null)
            {
                Type propertyType = property.objectReferenceValue.GetType();

                // Verificar se o tipo do asset � suportado (GameObject ou Sprite)
                if (propertyType == typeof(GameObject) || propertyType == typeof(Sprite))
                {
                    // Verificar se h� pr�-visualiza��o dispon�vel
                    Texture assetPreview = AssetPreview.GetAssetPreview(property.objectReferenceValue);
                    if (assetPreview != null)
                    {
                        return base.GetPropertyHeight(property, label) + 100 + 2; // Aumentar a altura para incluir a pr�-visualiza��o
                    }
                    else
                    {
                        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + 2; // Altura para incluir mensagem de aviso
                    }
                }
                else
                {
                    return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + 2; // Altura para incluir mensagem de aviso
                }
            }
            else
            {
                return base.GetPropertyHeight(property, label); // Altura padr�o sem pr�-visualiza��o
            }
        }
    }
#endif
}