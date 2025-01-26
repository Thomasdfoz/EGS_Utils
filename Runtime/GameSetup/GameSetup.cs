using System;
using UnityEngine;



namespace EGS.Utils 
{
    [CreateAssetMenu(fileName = "GameSetup", menuName = "Game Setup/Create Game Setup")]
    public class GameSetup : ScriptableObject
    {
        [System.Serializable]
        public class Variable
        {
            public string Key;
            public string ActionTypeName;
            public string ActionFullTypeName;

            [SerializeField] private string VarType;
            [SerializeField] private string StringValue;
            [SerializeField] private int IntValue;
            [SerializeField] private float FloatValue;
            [SerializeField] private bool BoolValue;
            [SerializeField] private Vector3 Vector3Value;
            [SerializeField] private Transform TransformValue;
            [SerializeField] private GameObject GameObjectValue;
            [SerializeField] private Animator AnimatorValue;
            [SerializeField] private AnimatorOverrideController AnimatorOverrideControllerValue;
            [SerializeField] private AnimationClip AnimationClipValue;
            [SerializeField] private string Action;

            public GameSetupAction SetupAction { get;  private set; }

            public object GetValue()
            {
                switch (VarType)
                {
                    case "string":
                        return StringValue;
                    case "int":
                        return IntValue;
                    case "float":
                        return FloatValue;
                    case "bool":
                        return BoolValue;
                    case "Vector3":
                        return Vector3Value;
                    case "Transform":
                        return TransformValue;
                    case "GameObject":
                        return GameObjectValue;
                    case "Animator":
                        return AnimatorValue;
                    case "AnimatorOverrideController":
                        return AnimatorOverrideControllerValue;
                    case "AnimationClip":
                        return AnimationClipValue;
                    default:
                        return null;
                }
            }

            public void SetValue(object value)
            {
                switch (VarType)
                {
                    case "string":
                        StringValue = value.ToString();
                        break;
                    case "int":
                        IntValue = (int) value;
                        break;
                    case "float":
                        FloatValue =(float) value;
                        break;
                    case "bool":
                         BoolValue = (bool) value;
                        break;
                    case "Vector3":
                        Vector3Value = (Vector3) value;
                        break;
                    case "Transform":
                        TransformValue = (Transform) value;
                        break;
                    case "GameObject":
                        GameObjectValue = (GameObject)value;
                        break;
                    case "Animator":
                        AnimatorValue = (Animator)value;
                        break;
                    case "AnimatorOverrideController":
                        AnimatorOverrideControllerValue = (AnimatorOverrideController)value;
                        break;
                    case "AnimationClip":
                        AnimationClipValue = (AnimationClip)value;
                        break;
                }
            }

            public void SetSetupAction(Type type)
            {
                SetupAction = (GameSetupAction)Activator.CreateInstance(type);
            }
        }

        public Variable[] variables;

        public Variable GetVariable(string key)
        {
            for (int i = 0; i < variables.Length; i++)
            {
                if (variables[i].Key == key)
                {
                    return variables[i];
                }
            }

            Debugger.LogError($"the variable:{key} is not registered or there is no action!");
            return null;
        }

        public GameSetupAction GetVariableAction(string key)
        {
            for (int i = 0; i < variables.Length; i++)
            {
                if (variables[i].Key == key)
                {
                    if (!string.IsNullOrEmpty(variables[i].ActionTypeName))
                        return variables[i].SetupAction;
                }
            }

            Debugger.LogError($"the variable:{key} is not registered!");
            return null;
        }

        public void InitializeVariables()
        {
            foreach (var variable in variables)
            {
                if (!string.IsNullOrEmpty(variable.ActionFullTypeName))
                {
                    Type type = Type.GetType(variable.ActionFullTypeName);
                    if (type != null)
                    {
                        variable.SetSetupAction(type);
                    }
                }
            }
        }

        /*#if UNITY_EDITOR

                public void SetupEnvironmentVariables()
                {
                    foreach (var variable in variables)
                    {
                        SetEnvironmentVariable(variable.Key, variable.Value);
                    }
                }

                private void SetEnvironmentVariable(string Key, string Value)
                {

                    Environment.SetEnvironmentVariable(Key, Value);

                }

                [ContextMenu("Execute SetupEnvironmentVariables")]
                public void Validate()
                {
                    try
                    {
                        SetupEnvironmentVariables();
                        Debug.Log($"Adicionado variaveis de ambiente com sucesso!!!");
                    }
                    catch (Exception error)
                    {
                        Debug.LogError($"Error when try to update icon with error: {error}");
                        throw;
                    }
                }

                [ContextMenu("ListAllEnvironmentVariables")]
                public void ListAllEnvironmentVariables()
                {
                    // Obt�m todas as vari�veis de ambiente
                    IDictionary environmentVariables = Environment.GetEnvironmentVariables();

                    Debug.Log($"COUNT:: {environmentVariables.Count}");

                    // Converte para uma lista de pares de chave/valor e ordena por chave
                    var sortedEnvironmentVariables = new List<DictionaryEntry>();

                    foreach (DictionaryEntry variable in environmentVariables)
                    {
                        sortedEnvironmentVariables.Add(variable);
                    }

                    // Ordena a lista pela chave
                    sortedEnvironmentVariables.Sort((x, y) => string.Compare(x.Key.ToString(), y.Key.ToString(), StringComparison.Ordinal));

                    // Itera sobre as vari�veis de ambiente ordenadas e exibe seus nomes e valores
                    foreach (DictionaryEntry variable in sortedEnvironmentVariables)
                    {
                        string Key = variable.Key.ToString();
                        string Value = variable.Value.ToString();
                        Debug.Log($"{Key} = {Value}");
                    }
                }


                [ContextMenu("ClearEnvironmentVariables")]
                public void ClearEnvironmentVariables()
                {
                    foreach (var variable in variables)
                    {
                        if (!string.IsNullOrEmpty(variable.Key))
                        {
                            Environment.SetEnvironmentVariable(variable.Key, null);
                            Debug.Log($"Vari�vel de ambiente removida: {variable.Key}");
                        }
                    }
                    Debug.Log("Todas as vari�veis de ambiente configuradas foram removidas.");
                }
        #endif*/
    }
}