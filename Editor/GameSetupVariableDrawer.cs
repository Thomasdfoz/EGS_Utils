#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EGS.Utils.Editor 
{
    [CustomPropertyDrawer(typeof(GameSetup.Variable))]
    public class GameSetupVariableDrawer : PropertyDrawer
    {
        private string[] actionTypeNames;
        private string[] actionFullTypeNames;
        private string[] predefinedKeys;
        private string[] varTypeOptions;
        private Rect keyrec;
        public GameSetupVariableDrawer()
        {
            InitializeActionTypeNames();
            InitializePredefinedKeys();
            InitializeVarTypeOptions();
        }

        private void InitializeActionTypeNames()
        {
            List<Type> actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(GameSetupAction)) && !type.IsAbstract)
                .ToList();

            actionTypeNames = new string[] { "None" }.Concat(actionTypes.Select(type => type.Name)).ToArray();
            actionFullTypeNames = new string[] { "None" }.Concat(actionTypes.Select(type => type.FullName)).ToArray();
        }

        private void InitializePredefinedKeys()
        {
            predefinedKeys = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(GameSetupVariableKeys).IsAssignableFrom(type))
                .SelectMany(type => type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                                        .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                                        .Select(field => field.Name))
                .ToArray();
        }

        private void InitializeVarTypeOptions()
        {
            varTypeOptions = new string[]
            {
                "None",
                "string",
                "int",
                "float",
                "bool",
                "Vector3",
                "Transform",
                "GameObject",
                "Animator",
                "AnimatorOverrideController",
                "AnimationClip"
                // Adicione mais tipos conforme necess�rio
            };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            DrawKeyDropdown(position, property);
            DrawVarTypeDropdown(position, property);
            DrawValueField(position, property);
            DrawActionDropdown(position, property);

            EditorGUI.EndProperty();
        }

        private void DrawKeyDropdown(Rect position, SerializedProperty property)
        {
            var keyProperty = property.FindPropertyRelative("Key");
            keyrec = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            int keyIndex = Array.IndexOf(predefinedKeys, keyProperty.stringValue);
            keyIndex = EditorGUI.Popup(keyrec, "Key", keyIndex < 0 ? 0 : keyIndex, predefinedKeys);
            keyProperty.stringValue = predefinedKeys.Length > 0 ? predefinedKeys[keyIndex] : string.Empty;
        }

        private void DrawVarTypeDropdown(Rect position, SerializedProperty property)
        {
            var varTypeProperty = property.FindPropertyRelative("VarType");
            Rect varTypeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

            int typeIndex = Array.IndexOf(varTypeOptions, varTypeProperty.stringValue);
            typeIndex = EditorGUI.Popup(varTypeRect, "Type", typeIndex < 0 ? 0 : typeIndex, varTypeOptions);
            varTypeProperty.stringValue = varTypeOptions[typeIndex];
        }

        private void DrawValueField(Rect position, SerializedProperty property)
        {
            var varTypeProperty = property.FindPropertyRelative("VarType");

            Rect valueRect = new Rect(position.x, position.y + 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), position.width, EditorGUIUtility.singleLineHeight);

            Type selectedType = GetTypeFromVarType(varTypeProperty.stringValue);
            SerializedProperty ValueProperty = null;

            switch (selectedType?.Name)
            {
                case "Int32":
                    ValueProperty = FindProperty("IntValue", property);
                    ValueProperty.intValue = EditorGUI.IntField(valueRect, "Value", ValueProperty.intValue);
                    break;
                case "Single":
                    ValueProperty = FindProperty("FloatValue", property);
                    ValueProperty.floatValue = EditorGUI.FloatField(valueRect, "Value", ValueProperty.floatValue);
                    break;
                case "String":
                    ValueProperty = FindProperty("StringValue", property);
                    ValueProperty.stringValue = EditorGUI.TextField(valueRect, "Value", ValueProperty.stringValue);
                    break;
                case "Boolean":
                    ValueProperty = FindProperty("BoolValue", property);
                    ValueProperty.boolValue = EditorGUI.Toggle(valueRect, "Value", ValueProperty.boolValue);
                    break;
                case "Vector3":
                    ValueProperty = FindProperty("Vector3Value", property);
                    ValueProperty.vector3Value = EditorGUI.Vector3Field(valueRect, "Value", ValueProperty.vector3Value);
                    break;
                case "Transform":
                    ValueProperty = FindProperty("TransformValue", property);
                    ValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, "Value", ValueProperty.objectReferenceValue, typeof(Transform), true);
                    break;
                case "GameObject":
                    ValueProperty = FindProperty("GameObjectValue", property);
                    ValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, "Value", ValueProperty.objectReferenceValue, typeof(GameObject), true);
                    break;
                case "Animator":
                    ValueProperty = FindProperty("AnimatorValue", property);
                    ValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, "Value", ValueProperty.objectReferenceValue, typeof(Animator), true);
                    break;
                case "AnimatorOverrideController":
                    ValueProperty = FindProperty("AnimatorOverrideControllerValue", property);
                    ValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, "Value", ValueProperty.objectReferenceValue, typeof(AnimatorOverrideController), true);
                    break;
                case "AnimationClip":
                    ValueProperty = FindProperty("AnimationClipValue", property);
                    ValueProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, "Value", ValueProperty.objectReferenceValue, typeof(AnimationClip), true);
                    break;
                default:
                    EditorGUI.LabelField(valueRect, "Value", "Unsupported Type");
                    break;
            }
        }

        private SerializedProperty FindProperty(string nameProperty, SerializedProperty property)
        {
            return property.FindPropertyRelative(nameProperty);
        }

        private void DrawActionDropdown(Rect position, SerializedProperty property)
        {
            var actionTypeNameProperty = property.FindPropertyRelative("ActionTypeName");
            var actionFullTypeNameProperty = property.FindPropertyRelative("ActionFullTypeName");

            var actionProperty = property.FindPropertyRelative("Action");
            Rect ActionRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3, position.width, EditorGUIUtility.singleLineHeight);

            int actionIndex = Array.IndexOf(actionTypeNames, actionTypeNameProperty.stringValue);
            actionIndex = EditorGUI.Popup(ActionRect, "Action", actionIndex < 0 ? 0 : actionIndex, actionTypeNames);

            actionTypeNameProperty.stringValue = actionTypeNames[actionIndex] == "None" ? string.Empty : actionTypeNames[actionIndex];
            actionFullTypeNameProperty.stringValue = actionFullTypeNames[actionIndex] == "None" ? string.Empty : actionFullTypeNames[actionIndex];
        }

        private Type GetTypeFromVarType(string varType)
        {
            switch (varType)
            {
                case "int": return typeof(int);
                case "float": return typeof(float);
                case "string": return typeof(string);
                case "bool": return typeof(bool);
                case "Vector3": return typeof(Vector3);
                case "Transform": return typeof(Transform);
                case "GameObject": return typeof(GameObject);
                case "Animator": return typeof(Animator);
                case "AnimatorOverrideController": return typeof(AnimatorOverrideController);
                case "AnimationClip": return typeof(AnimationClip);
                default: return null;
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 5 * EditorGUIUtility.singleLineHeight + 4 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif
