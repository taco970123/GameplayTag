using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptableObjectField : VisualElement
{
    Label m_TitleLabel;
    ObjectField m_ObjectField;
    VisualElement m_ScriptableObjectView;

    SerializedProperty m_SerializedProperty;
    UnityEngine.Object m_Owner;

    public ScriptableObjectField(Type targetType, string bindPath, SerializedProperty serializedProperty)
    {
        var visualTree = Resources.Load<VisualTreeAsset>("VisualTree/ScriptableObjectField");
        visualTree?.CloneTree(this);
        AddToClassList("scriptableObjectField");

        s_Owners.Add(serializedProperty.serializedObject.targetObject);
        m_SerializedProperty = serializedProperty;
        m_Owner = serializedProperty.serializedObject.targetObject;

        VisualElement top = new VisualElement();
        top.style.flexDirection = FlexDirection.Row;
        top.name = "top";
        Add(top);

        m_TitleLabel = new Label(serializedProperty.displayName);
        m_TitleLabel.AddManipulator(new Clickable(() => m_ScriptableObjectView.style.display = m_ScriptableObjectView.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex));
        m_TitleLabel.AddToClassList("title");
        top.Add(m_TitleLabel);

        m_ObjectField = new ObjectField();
        m_ObjectField.name = "objectField";
        m_ObjectField.objectType = targetType;
        m_ObjectField.bindingPath = bindPath;
        m_ObjectField.Bind(m_SerializedProperty.serializedObject);
        m_ObjectField.RegisterValueChangedCallback(OnValueChanged);
        top.Add(m_ObjectField);

        m_ScriptableObjectView = new VisualElement();
        m_ScriptableObjectView.name = "scriptableObjectView";
        Add(m_ScriptableObjectView);

        RegisterCallback<DetachFromPanelEvent>(OnDestroy);

    }

    void OnDestroy(DetachFromPanelEvent e)
    {
        s_Owners.Remove(m_Owner);
        UnregisterCallback<DetachFromPanelEvent>(OnDestroy);
    }
    void OnValueChanged(ChangeEvent<UnityEngine.Object> changeEvent)
    {
        UnityEngine.Object newValue = changeEvent.newValue;
        schedule.Execute(() =>
        {
            m_TitleLabel.text = m_SerializedProperty.displayName;
            m_ScriptableObjectView.style.display = newValue ? DisplayStyle.Flex : DisplayStyle.None;
            m_ScriptableObjectView.Clear();
            if (s_Owners.Contains(newValue))
            {
                m_ObjectField.style.display = DisplayStyle.Flex;
                return;
            }
            else if (newValue)
            {
                m_TitleLabel.text = newValue.name;
                SerializedObject serializedObject = new SerializedObject(m_SerializedProperty.objectReferenceValue);

                //SerializedProperty serializedProperty = serializedObject.GetIterator();
                //serializedProperty.NextVisible(true);
                //while (serializedProperty.NextVisible(false))
                //{
                //    PropertyField propertyField = new PropertyField(serializedProperty);
                //    propertyField.Bind(serializedObject);
                //    m_ScriptableObjectContent.Add(propertyField);
                //}

                foreach (var item in GetFields(serializedObject.targetObject.GetType()))
                {
                    SerializedProperty serializedProperty = serializedObject.FindProperty(item.Name);
                    if (serializedProperty != null)
                    {
                        PropertyField propertyField = new PropertyField(serializedProperty);
                        propertyField.Bind(serializedObject);
                        m_ScriptableObjectView.Add(propertyField);
                    }
                }
            }
        });

        //m_TitleLabel.text = m_SerializedProperty.displayName;
        //m_ScriptableObjectView.style.display = newValue ? DisplayStyle.Flex : DisplayStyle.None;
        //m_ScriptableObjectView.Clear();
        //if (s_Owners.Contains(newValue))
        //{
        //    m_ObjectField.style.display = DisplayStyle.Flex;
        //    return;
        //}
        //else if (newValue)
        //{
        //    m_TitleLabel.text = newValue.name;
        //    SerializedObject serializedObject = new SerializedObject(m_SerializedProperty.objectReferenceValue);

        //    //SerializedProperty serializedProperty = serializedObject.GetIterator();
        //    //serializedProperty.NextVisible(true);
        //    //while (serializedProperty.NextVisible(false))
        //    //{
        //    //    PropertyField propertyField = new PropertyField(serializedProperty);
        //    //    propertyField.Bind(serializedObject);
        //    //    m_ScriptableObjectContent.Add(propertyField);
        //    //}

        //    foreach (var item in GetFields(serializedObject.targetObject.GetType()))
        //    {
        //        SerializedProperty serializedProperty = serializedObject.FindProperty(item.Name);
        //        if (serializedProperty != null)
        //        {
        //            PropertyField propertyField = new PropertyField(serializedProperty);
        //            propertyField.Bind(serializedObject);
        //            m_ScriptableObjectView.Add(propertyField);
        //        }
        //    }
        //}
    }

    static List<UnityEngine.Object> s_Owners = new List<UnityEngine.Object>();
    static Dictionary<Type, FieldInfo[]> s_FieldInfoMapCached = new Dictionary<Type, FieldInfo[]>();
    public static FieldInfo[] GetFields(Type type)
    {
        if (s_FieldInfoMapCached.ContainsKey(type))
        {
            return s_FieldInfoMapCached[type];
        }
        else
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            s_FieldInfoMapCached.Add(type, fields);
            return fields;
        }
    }
}

[CustomPropertyDrawer(typeof(ScriptableObjectFieldAttribute), true)]
public class ScriptableObjectDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var attribute = fieldInfo.GetCustomAttribute<ScriptableObjectFieldAttribute>();
        ScriptableObjectField scriptableObjectField = new ScriptableObjectField(attribute.Type, property.propertyPath, property);
        return scriptableObjectField;
    }
}

[CustomPropertyDrawer(typeof(ObjectFielAttribute), true)]
public class ObjectFieldDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var attribute = fieldInfo.GetCustomAttribute<ObjectFielAttribute>();
        ObjectField propertyField = new ObjectField();
        propertyField.bindingPath = property.propertyPath;
        propertyField.Bind(property.serializedObject);

        var method = property.serializedObject.targetObject.GetType().GetMethod(attribute.OnValueChangedCallback, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
        if(method != null)
        {
            propertyField.RegisterValueChangedCallback((i) =>
            {
                method.Invoke(property.serializedObject.targetObject, new object[] { i.newValue });
            });
        }
        return propertyField;
    }
}