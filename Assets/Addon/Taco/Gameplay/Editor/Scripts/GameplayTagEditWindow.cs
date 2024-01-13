using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public class GameplayTagEditWindow : EditorWindow
    {
        TextField m_TagNameField;
        Button m_AddTagButton;
        VisualElement m_Container;
        
        BiDictionary<string, FolderView> m_GameplayTagInfoMap = new BiDictionary<string, FolderView>();

        bool m_Moving;
        bool m_Clicked;
        bool m_Renaming;
        float m_DoubleClickTimer = 0.2f;
        string m_MovingTag;
        FolderView m_RenamingFolderView;
        EditorCoroutine m_DoubleClickCoroutine;

        GameplayTagEditData m_EditData => GameplayTagEditorUtility.GameplayTagEditData;
        UndoHelper m_EditDataUndoHelper => GameplayTagEditorUtility.EditorDataUndoHelper;

        public virtual void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("VisualTree/GameplayTagEditWindow");
            visualTree.CloneTree(root);

            titleContent = new GUIContent("TagEditor");

            m_TagNameField = root.Q<TextField>("tag-name-field");
            m_TagNameField.selectAllOnMouseUp = true;

            m_AddTagButton = root.Q<Button>("add-tag-button");
            m_AddTagButton.clicked += () => AddTag(m_TagNameField.value);

            m_Container = root.Q("tag-container");

            m_EditData.Init();
            m_EditData.OnValueChanged += PopulateView;
            GameplayTagEditorUtility.RegisterUndo(UndoRedo);

            PopulateView();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        public virtual void OnDisable()
        {
            GameplayTagEditorUtility.UnregisterUndo(UndoRedo);
            m_EditData.OnValueChanged -= PopulateView;
        }
        public virtual void OnFocus()
        {
   
        }
        public virtual void OnLostFocus()
        {
            StopMoving();
            StopRenaming();
        }
        private void Update()
        {
            if (m_Moving && rootVisualElement.focusController.focusedElement != null
                         && focusedWindow.GetType().ToString() != "UnityEditor.UIElements.EditorMenuExtensions+ContextMenu")
            {
                StopMoving();
            }
            if (m_Renaming && rootVisualElement.focusController.focusedElement != null 
                           && rootVisualElement.focusController.focusedElement != m_RenamingFolderView.FolderNameField
                           && rootVisualElement.focusController.focusedElement != m_RenamingFolderView.ConfirmButton)
            {
                StopRenaming();
            }
            if (EditorApplication.isCompiling)
            {
                Close();
            }
        }
       
        void PopulateView()
        {
            m_Container.Clear();
            m_GameplayTagInfoMap.Clear();

            foreach (var gameplayTagEditInfo in m_EditData.GameplayTagEditInfos)
            {
                string tag = gameplayTagEditInfo.Name;
                var splitStrings = tag.Split('.');
                string shortTag = splitStrings[splitStrings.Length - 1];

                FolderView folderView = new FolderView(shortTag, tag, (menu) =>
                {
                    menu.AppendAction("Move Tag", (a) =>
                    {
                        m_Moving = true;
                        m_MovingTag = tag;
                        m_TagNameField.value = $"Moving: {tag}";
                    });
                    menu.AppendAction("Move to Root", (a) =>
                    {
                        MoveToRoot(tag);
                    });
                    menu.AppendAction("Remove Tag", (a) => RemoveTag(tag));
                    menu.AppendAction("Remove Tag without Children", (a) => RemoveTagWithoutChildren(tag));
                });

                StyleSheet style = Resources.Load<StyleSheet>("StyleSheet/GameplayTagFolder");
                folderView.styleSheets.Add(style);

                folderView.Label.AddManipulator(new Clickable(() =>
                {
                    if (m_Moving)
                    {
                        MoveTag(folderView);
                        StopMoving();
                    }
                    else
                    {
                        if (m_Clicked == true && m_RenamingFolderView == folderView)
                        {
                            m_Clicked = false;
                            m_Renaming = true;
                            folderView.RefreshRenameState(true);
                            folderView.OnConfirmed += () =>
                            {
                                ChangeTag();
                                StopRenaming();
                            };
                            EditorCoroutineUtility.StopCoroutine(m_DoubleClickCoroutine);
                        }
                        else
                        {
                            m_TagNameField.value = m_GameplayTagInfoMap.Reverse[folderView];
                            {
                                m_Clicked = true;
                                m_Renaming = false;
                                m_RenamingFolderView?.RefreshRenameState(false);
                                m_RenamingFolderView = folderView;
                                m_DoubleClickCoroutine = EditorCoroutineUtility.StartCoroutine(DoubleClickCoroutine(m_DoubleClickTimer), this);
                            }
                        }
                    }
                }));
                folderView.Expanded = m_EditData[tag].Expanded;
                folderView.OnExpandedStateChanged += () => GameplayTagEditorUtility.SetExpandedState(tag, folderView.Expanded);

                if (splitStrings.Length == 1)
                {
                    m_Container.Add(folderView);
                    folderView.AddToClassList("rootFolder");
                }
                else
                {
                    string parentPath = tag.Substring(0, Mathf.Max(tag.Length - shortTag.Length - 1, 0));
                    if (m_GameplayTagInfoMap.TryGetValue(parentPath, out FolderView parentView))
                        parentView.AddContent(folderView);
                }

                m_GameplayTagInfoMap.Add(tag, folderView);
            }
        }
        void AddTag(string newTag)
        {
            if(newTag == string.Empty)
            {
                WarningWindow.Show("Error", "Input is empty", m_AddTagButton);
                return;
            }
            else if (m_EditData.Contains(newTag))
            {
                WarningWindow.Show("Error", "Name exists", m_AddTagButton);
                return;
            }
            else if (newTag.StartsWith('.') || newTag.EndsWith('.'))
            {
                WarningWindow.Show("Error", "Input format error", m_AddTagButton);
                return;
            }
            for (int i = 0; i < newTag.Length - 1; i++)
            {
                if (newTag[i] == newTag[i + 1] && newTag[i] == '.')
                {
                    WarningWindow.Show("Error", "Input format error", m_AddTagButton);
                    return;
                }
            }

            m_TagNameField.value = m_TagNameField.value.Replace('¡£', '.');
            m_TagNameField.value = m_TagNameField.value.Replace('/', '.');

            m_EditDataUndoHelper.Do(() => m_EditData.AddTag(m_TagNameField.value), "Add Tag");
        }
        void RemoveTagWithoutChildren(string tagToRemove)
        {
            m_EditDataUndoHelper.Do(() => m_EditData.RemoveTagWithoutChildren(tagToRemove), "Remove Tag");
        }
        void RemoveTag(string tagToRemove)
        {
            string dialogContext = string.Empty;

            List<GameplayTagEditInfo> childTags = m_EditData.GetChildTagInfos(tagToRemove, true);
            childTags.ForEach(i => dialogContext += i.Name + "\n");

            WarningWindow.Show("Confirm to remove".ToUpper(), dialogContext, () =>
            {
                m_EditDataUndoHelper.Do(() => m_EditData.RemoveTag(tagToRemove), "Remove Tag");
            }, null, GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
        }
        void ChangeTag()
        {
            string oldShortTag = m_RenamingFolderView.Name;
            string oldTag = m_RenamingFolderView.DetailName;
            string newShortTag = m_RenamingFolderView.NameFieldValue;
            if (newShortTag == string.Empty)
            {
                return;
            }
            else if (newShortTag == oldShortTag)
            {
                return;
            }
            else if (newShortTag.Contains('.'))
            {
                WarningWindow.Show("Error", "Input format error", m_RenamingFolderView);
                return;
            }
            string newTag = oldTag.Remove(oldTag.Length - oldShortTag.Length,oldShortTag.Length) + newShortTag;
            m_EditDataUndoHelper.Do(() => m_EditData.ChangeTag(oldTag, newTag, newShortTag), "Change Tag");
        }
        void MoveTag(FolderView folderView)
        {
            string targetParentTag = m_GameplayTagInfoMap.Reverse[folderView];
            if (m_MovingTag == targetParentTag)
            {
                return;
            }
            else if (m_EditData.GetChildTagInfos(m_MovingTag, false).Find(i => i.Name == targetParentTag) is GameplayTagEditInfo gameplayTagEditInfo)
            {
                WarningWindow.Show("Error", "Cannot move to child", folderView);
                return;
            }
            m_EditDataUndoHelper.Do(() => m_EditData.MoveTag(m_MovingTag, targetParentTag), "Move Tag");
        }
        void MoveToRoot(string movingTag)
        {
            m_EditDataUndoHelper.Do(() => m_EditData.MoveToRoot(movingTag), "Move to Root");
        }

        void StopMoving()
        {
            m_Moving = false;
            m_MovingTag = string.Empty;
            if(m_TagNameField != null)
                m_TagNameField.value = "tag name";
        }
        void StopRenaming()
        {
            foreach (var item in m_GameplayTagInfoMap)
            {
                item.Value.RefreshRenameState(false);
            }
            m_Renaming = false;
            m_Clicked = false;
            m_RenamingFolderView = null;
            if (m_DoubleClickCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(m_DoubleClickCoroutine);
        }
        IEnumerator DoubleClickCoroutine(float timer)
        {
            yield return new EditorWaitForSeconds(timer);
            m_Clicked = false;
        }
        void UndoRedo()
        {
            Focus();
            PopulateView();
        }

        bool inited;
        public Vector2 initialPos;
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            rootVisualElement.schedule.Execute(() =>
            {
                if (!inited)
                {
                    if(initialPos != Vector2.zero && rootVisualElement.worldBound.width <= m_Container.worldBound.width)
                        position = new Rect(initialPos, new Vector2(m_Container.worldBound.width, position.height));

                    minSize = new Vector2(m_Container.worldBound.width, position.height);
                    inited = true;
                }
            });
        }


        [MenuItem("Tools/Gameplay/GameplayTagEditWindow", false, 0)]
        public static void OpenGameplayTagEditWindow()
        {
            GetWindow<GameplayTagEditWindow>();
        }
        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            if (HasOpenInstances<GameplayTagEditWindow>())
                GetWindow<GameplayTagEditWindow>().Close();
        }
    }
}