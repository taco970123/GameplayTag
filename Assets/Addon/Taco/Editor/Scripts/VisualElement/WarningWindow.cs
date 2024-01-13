using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Taco.Editor
{
    public class WarningWindow : EditorWindow
    {
        Label m_Context;
        VisualElement m_Bar;
        Button m_ConfirmButton;
        Button m_CancelButton;

        Vector2 m_Position;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("VisualTree/WarningWindow");
            visualTree.CloneTree(root);
            root.AddToClassList("warningWindow");

            m_Context = root.Q<Label>("context");
            m_Bar = root.Q("bar");
            m_ConfirmButton = root.Q<Button>("confirm-button");
            m_CancelButton = root.Q<Button>("cancel-button");

            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        public void Init(string title, string context, string confirm, string cancel, Action confirmCallback,Action cancelCallback)
        {
            titleContent = new GUIContent(title, EditorGUIUtility.IconContent("console.warnicon").image);
            m_Context.text = context;
            m_ConfirmButton.text = confirm;
            m_ConfirmButton.clicked += ()=> 
            {
                confirmCallback?.Invoke();
                Close();
            };
            m_CancelButton.text = cancel;
            m_CancelButton.clicked += ()=> 
            {
                cancelCallback?.Invoke();
                Close();
            };
        }
        public void Init(string title, string context)
        {
            titleContent = new GUIContent(title, EditorGUIUtility.IconContent("console.warnicon").image);
            m_Context.text = context;
            m_ConfirmButton.style.display = DisplayStyle.None;
            m_CancelButton.clicked += Close;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Rect rect = new Rect(m_Position, new Vector2(position.size.x, m_Context.worldBound.height + m_Bar.worldBound.height));
            position = rect;
            minSize = maxSize = rect.size;
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public static WarningWindow Show(string title, string context, VisualElement button)
        {
            float xMin = GUIUtility.GUIToScreenPoint(button.worldBound.position).x;
            float yMin = GUIUtility.GUIToScreenPoint(button.worldBound.position).y + button.worldBound.height + 30;
            return Show(title, context, new Vector2(xMin, yMin));
        }
        public static WarningWindow Show(string title, string context,Vector2 screenPosition)
        {
            WarningWindow warningWindow = GetWindow<WarningWindow>();
            warningWindow.Init(title, context);
            warningWindow.m_Position = screenPosition;
            warningWindow.ShowModal();
            return warningWindow;
        }

        public static WarningWindow Show(string title, string context, Action confirmCallback, Action cancelCallback, VisualElement button)
        {
            return Show(title, context, "Confirm", "Cancel", confirmCallback, cancelCallback, button);
        }
        public static WarningWindow Show(string title, string context, Action confirmCallback, Action cancelCallback, Vector2 screenPosition)
        {
            return Show(title, context, "Confirm", "Cancel", confirmCallback, cancelCallback, screenPosition);
        }

        public static WarningWindow Show(string title, string context, string confirm, string cancel, Action confirmCallback, Action cancelCallback, VisualElement button)
        {
            float xMin = GUIUtility.GUIToScreenPoint(button.worldBound.position).x;
            float yMin = GUIUtility.GUIToScreenPoint(button.worldBound.position).y + button.worldBound.height + 30;
            return Show(title, context, confirm, cancel, confirmCallback, cancelCallback, new Vector2(xMin, yMin));
        }
        public static WarningWindow Show(string title, string context, string confirm, string cancel, Action confirmCallback, Action cancelCallback, Vector2 screenPosition)
        {
            WarningWindow warningWindow = GetWindow<WarningWindow>();
            warningWindow.Init(title, context, confirm, cancel, confirmCallback, cancelCallback);
            warningWindow.m_Position = screenPosition;
            warningWindow.ShowModal();
            return warningWindow;
        }
    }
}