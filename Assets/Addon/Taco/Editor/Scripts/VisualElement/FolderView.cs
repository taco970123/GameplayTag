using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Taco.Editor
{
    public class FolderView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<FolderView, UxmlTraits> { }

        protected VisualElement m_Top;
        public VisualElement Top => m_Top;

        protected Label m_Label;
        public Label Label => m_Label;

        protected Label m_DetailLabel;
        public Label DetailLabel => m_DetailLabel;

        protected VisualElement m_FolderButton;
        public VisualElement FolderButton => m_FolderButton;

        protected VisualElement m_ExpandIcon;
        public VisualElement ExpandIcon => m_ExpandIcon;

        protected TextField m_FolderNameField;
        public TextField FolderNameField => m_FolderNameField;

        protected Button m_ConfirmButton;
        public Button ConfirmButton => m_ConfirmButton;

        protected VisualElement m_OptionButton;
        public VisualElement OptionButton => m_OptionButton;

        protected VisualElement m_Content;
        public VisualElement Content => m_Content;


        const string m_VisualTreeAssetGUID = "9b7ee77f19ad4c94aafe06f2d24e200d";

        bool m_Expanded;
        public bool Expanded
        {
            get => m_Expanded;
            set
            {
                m_Expanded = value;
                RefreshExpandedState();
                OnExpandedStateChanged?.Invoke();
            }
        }

        public string Name { get; private set; }
        public string DetailName { get; private set; }
        public string NameFieldValue => m_FolderNameField.value;

        public Action OnExpandedStateChanged;
        public Action OnConfirmed;
        public Action OnDeleted;


        public FolderView() : this("Folder", "DetailFolder", null) { }
        public FolderView(string name) : this(name, name, null) { }
        public FolderView(string name, string detailName, Action<DropdownMenu> menuBuilder) : this(m_VisualTreeAssetGUID, name, detailName, menuBuilder) { }
        public FolderView(string guid, string name, string detailName, Action<DropdownMenu> menuBuilder)
        {
            VisualTreeAsset template = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(VisualTreeAsset)) as VisualTreeAsset;
            template.CloneTree(this);
            AddToClassList("folderView");

            Name = name;
            DetailName = detailName;

            m_Top = this.Q("top");

            m_Label = this.Q<Label>("folder-title");
            m_Label.text = name;

            m_DetailLabel = this.Q<Label>("folder-detail-label");
            m_DetailLabel.text = detailName;
            if (detailName != name)
                m_DetailLabel.AddToClassList("detailLabel");

            m_FolderButton = this.Q("folder-button");
            m_FolderButton.AddManipulator(new Clickable(ToggleCollapsed));

            m_ExpandIcon = this.Q("folder-icon");

            m_FolderNameField = this.Q<TextField>("folder-rename-field");
            m_FolderNameField.value = name;
            m_FolderNameField.selectAllOnMouseUp = true;

            m_ConfirmButton = this.Q<Button>("confirm-button");
            m_ConfirmButton.clicked += () => OnConfirmed?.Invoke();

            m_OptionButton = this.Q("option-button");
            if (menuBuilder != null)
            {
                m_OptionButton.AddManipulator(new DropdownMenuManipulator(menuBuilder, MouseButton.LeftMouse));
                m_OptionButton.AddToClassList("optional");
            }

            m_Content = this.Q("content");
            RefreshExpandedState();
        }

        public void RefreshRenameState(bool value)
        {
            if (value)
            {
                m_Top.AddToClassList("renaming");
                m_Label.AddToClassList("renaming");
                m_DetailLabel.AddToClassList("renaming");
                m_FolderNameField.AddToClassList("renaming");
                m_ConfirmButton.AddToClassList("renaming");
                m_OptionButton.AddToClassList("renaming");
                m_FolderNameField.SelectAll();
            }
            else
            {
                m_Top.RemoveFromClassList("renaming");
                m_Label.RemoveFromClassList("renaming");
                m_DetailLabel.RemoveFromClassList("renaming");
                m_FolderNameField.RemoveFromClassList("renaming");
                m_ConfirmButton.RemoveFromClassList("renaming");
                m_OptionButton.RemoveFromClassList("renaming");
                OnConfirmed = null;
            }
        }
        public void AddContent(VisualElement content)
        {
            m_Content.Add(content);
            AddToClassList("hasChild");
            m_Top.AddToClassList("hasChild");
        }

        void ToggleCollapsed()
        {
            Expanded = !Expanded;
        }
        void RefreshExpandedState()
        {
            m_FolderButton.RemoveFromClassList("expanded");
            m_FolderButton.RemoveFromClassList("collapsed");
            m_ExpandIcon.RemoveFromClassList("expanded");
            m_ExpandIcon.RemoveFromClassList("collapsed");
            m_Content.RemoveFromClassList("expanded");
            m_Content.RemoveFromClassList("collapsed");

            if (m_Expanded)
            {
                m_FolderButton.AddToClassList("expanded");
                m_ExpandIcon.AddToClassList("expanded");
                m_Content.AddToClassList("expanded");
            }
            else
            {
                m_FolderButton.AddToClassList("collapsed");
                m_ExpandIcon.AddToClassList("collapsed");
                m_Content.AddToClassList("collapsed");
            }
        }
    }
}