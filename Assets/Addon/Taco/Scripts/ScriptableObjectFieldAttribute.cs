using System;
using UnityEngine;

public class ScriptableObjectFieldAttribute : PropertyAttribute
{
    public readonly Type Type;
    public ScriptableObjectFieldAttribute(Type type)
    {
        Type = type;
    }
}

public class ObjectFielAttribute : PropertyAttribute 
{
    public string OnValueChangedCallback;
    public ObjectFielAttribute(string onValueChangedCallback)
    {
        OnValueChangedCallback = onValueChangedCallback;
    }
}