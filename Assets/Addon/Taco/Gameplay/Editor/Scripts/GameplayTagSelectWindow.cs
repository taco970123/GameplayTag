using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public class GameplayTagSelectWindow : EditorWindow
    {
        Button m_ExpandAllButton;
        Button m_ClearSelectionButton;
        Button m_OpenTagEditorButton;
        VisualElement m_Container;

        UndoHelper m_UndoHelper;

        BiDictionary<string, GameplayTagInfoView> m_GameplayTagInfoViewMap = new BiDictionary<string, GameplayTagInfoView>();

        bool m_AllExpanded;

        public System.Action OnClosed;


        GameplayTagData m_TagData => GameplayTagEditorUtility.GameplayTagData;
        GameplayTagContainer m_TagContainer => GameplayTagEditorUtility.GameplayTagContainer;
        Object m_GameplayTagContainerOwner => GameplayTagEditorUtility.GameplayTagContainerOwner;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("VisualTree/GameplayTagSelectWindow");
            visualTree.CloneTree(root);

            titleContent = new GUIContent("TagSelector");

            m_ExpandAllButton = root.Q<Button>("expand-all-button");
            m_AllExpanded = true;
            foreach (var tagEditInfo in m_TagData.GameplayTagInfos)
            {
                if (!tagEditInfo.Expanded)
                    m_AllExpanded = false;
            }
            if (m_AllExpanded)
                m_ExpandAllButton.AddToClassList("allExpanded");
            m_ExpandAllButton.clicked += () =>
            {
                if (m_AllExpanded)
                {
                    foreach (var gameplayTagInfoViewPair in m_GameplayTagInfoViewMap)
                    {
                        if(!gameplayTagInfoViewPair.Value.ToggleButton.value)
                            gameplayTagInfoViewPair.Value.Expanded = false;
                    }                 
                }
                else
                {
                    foreach (var gameplayTagInfoViewPair in m_GameplayTagInfoViewMap)
                    {
                        gameplayTagInfoViewPair.Value.Expanded = true;
                    }
                    m_AllExpanded = true;
                    m_ExpandAllButton.AddToClassList("allExpanded");
                }
            };


            m_ClearSelectionButton = root.Q<Button>("clear-selection-button");
            m_ClearSelectionButton.clicked += () => m_UndoHelper.Do(() => m_TagContainer.ClearTags(), "Clear Selected Tags");

            m_OpenTagEditorButton = root.Q<Button>("open-editor-button");
            if (!HasOpenInstances<GameplayTagEditWindow>())
            {
                m_OpenTagEditorButton.clicked += () =>
                {
                    var gameplayTagEditWindow = GetWindow<GameplayTagEditWindow>();
                    gameplayTagEditWindow.Focus();
                    gameplayTagEditWindow.initialPos = position.position + new Vector2(-400, 0);
                };
            }

            m_Container = root.Q("tag-container");

            m_UndoHelper = new UndoHelper("GameplayTagContainer", UndoRedo, m_GameplayTagContainerOwner, m_TagData);
            GameplayTagEditorUtility.RegisterUndo(null);
            Selection.selectionChanged += Close;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            m_TagData.OnValueChanged += PopulateView;
            m_TagContainer.OnValueChanged += PopulateView;

            PopulateView();
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }
        public void OnDisable()
        {
            m_UndoHelper?.Dispose();
            GameplayTagEditorUtility.UnregisterUndo(null);
            Selection.selectionChanged -= Close;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            m_TagData.OnValueChanged -= PopulateView;
            m_TagContainer.OnValueChanged -= PopulateView;
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            OnClosed?.Invoke();
            OnClosed = null;
        }
        private void Update()
        {
            if (focusedWindow != null && focusedWindow != this && focusedWindow as GameplayTagEditWindow == null && focusedWindow.GetType().ToString() != "UnityEditor.UIElements.EditorMenuExtensions+ContextMenu")
            {
                Close();
            }
            if (EditorApplication.isCompiling)
            {
                Close();
            }
        }

        void PopulateView()
        {
            m_Container.Clear();
            m_GameplayTagInfoViewMap.Clear();

            foreach (var gameplayTagInfo in m_TagData.GameplayTagInfos)
            {
                AddGameplayTagInfoView(gameplayTagInfo.Name);
            }
            foreach (var tag in m_TagContainer.Tags)
            {
                if (!m_TagData.Contains(tag))
                {
                    AddRuntimeGameplayTagInfoView(tag);
                }
            }           
        }

        GameplayTagInfoView AddGameplayTagInfoView(string tag)
        {
            var splitStrings = tag.Split('.');
            string lastPath = splitStrings[splitStrings.Length - 1];

            GameplayTagInfoView gameplayTagInfoView = new GameplayTagInfoView(lastPath, tag, m_TagContainer.ContainsTag(tag));
            gameplayTagInfoView.Expanded = m_TagData[tag].Expanded;
            gameplayTagInfoView.OnExpandedStateChanged += () =>
            {
                GameplayTagEditorUtility.SetExpandedState(tag, gameplayTagInfoView.Expanded);
                if (!gameplayTagInfoView.Expanded)
                {
                    m_AllExpanded = false;
                    m_ExpandAllButton.RemoveFromClassList("allExpanded");
                }
            };
            gameplayTagInfoView.OnToggled = (i) =>
            {
                string tag = m_GameplayTagInfoViewMap.Reverse[gameplayTagInfoView];

                if (i.newValue)
                {
                    m_UndoHelper.Do(() =>
                    {
                        var parentTags = GameplayTagUtility.GetParentTags(tag);
                        for (int i = parentTags.Length - 1; i >= 0; i--)
                        {
                            if (m_TagData[parentTags[i]].Multi)
                                break;
                            else
                                m_TagContainer.RemoveTagWithChild(m_TagData.NameToInfo(parentTags[i]));
                        }
                        m_TagContainer.AddTag(m_TagData.NameToInfo(tag));
                    }, "Select Tag");
                }
                else
                {
                    m_UndoHelper.Do(() => 
                    {
                        m_TagContainer.RemoveTagWithChild(m_TagData.NameToInfo(tag));
                        var parentTags = Gameplay.GameplayTagUtility.GetParentTags(tag);
                        foreach (var parentTag in parentTags)
                        {
                            m_TagContainer.AddTag(m_TagData.NameToInfo(parentTag));
                        }
                    }, "Unselect Tag");
                }
            };

            if(splitStrings.Length == 1)
            {
                m_Container.Add(gameplayTagInfoView);
                gameplayTagInfoView.AddToClassList("rootFolder");
            }
            else
            {
                string parentPath = tag.Substring(0, Mathf.Max(tag.Length - lastPath.Length - 1, 0));
                if (m_GameplayTagInfoViewMap.TryGetValue(parentPath, out GameplayTagInfoView parentView))
                    parentView.AddContent(gameplayTagInfoView);
            }
            m_GameplayTagInfoViewMap.Add(tag, gameplayTagInfoView);
            return gameplayTagInfoView;
        }
        GameplayTagInfoView AddMissingGameplayTagInfoView(string tag)
        {
            var splitStrings = tag.Split('.');
            string lastPath = splitStrings[splitStrings.Length - 1];

            GameplayTagInfoView gameplayTagInfoView = new GameplayTagInfoView(lastPath + "  (missing)", tag, true);
            gameplayTagInfoView.Top.AddToClassList("missing");
            gameplayTagInfoView.Expanded = true;
            gameplayTagInfoView.OnToggled = (i) =>
            {
                if (i.newValue)
                    return;

                var childTags = Gameplay.GameplayTagUtility.GetChildrenTags(tag, m_TagContainer.Tags);
                List<string> probablyTags = new List<string>();
                probablyTags.Add(tag);
                foreach (var childTag in childTags)
                {
                    var middleTags = Gameplay.GameplayTagUtility.GetMiddleTags(tag, childTag);
                    foreach (var middleTag in middleTags)
                    {
                        if (!probablyTags.Contains(middleTag))
                            probablyTags.Add(middleTag);
                    }
                    probablyTags.Add(childTag);
                }

                string dialogContext = string.Empty;
                foreach (var probablyTag in probablyTags)
                {
                    dialogContext += probablyTag + '\n';
                }

                WarningWindow.Show("Confirm to delete", dialogContext,
                () =>
                {
                    m_UndoHelper.Do(() => 
                    {
                        m_TagContainer.RemoveTag(m_TagData.NameToInfo(tag));
                        foreach (var probablyTag in probablyTags)
                        {
                            m_TagContainer.RemoveTag(m_TagData.NameToInfo(probablyTag));
                        }
                        var parentTags = Gameplay.GameplayTagUtility.GetParentTags(tag);
                        if (parentTags.Length > 0)
                            m_TagContainer.AddTag(m_TagData.NameToInfo(parentTags[parentTags.Length - 1]));

                    }, "Unselect Tag");
                },
                () =>
                {
                    gameplayTagInfoView.ToggleButton.value = true;
                },
                gameplayTagInfoView.ToggleButton);
            };

            gameplayTagInfoView.RepairButton.clicked += () => GameplayTagEditorUtility.DataUndoHelper.Do(() => m_TagData.AddTag(tag), "Repair Tag");

            if (splitStrings.Length == 1)
            {
                m_Container.Add(gameplayTagInfoView);
                gameplayTagInfoView.AddToClassList("rootFolder");
            }
            else
            {
                string parentPath = tag.Substring(0, Mathf.Max(tag.Length - lastPath.Length - 1, 0));
                if (m_GameplayTagInfoViewMap.TryGetValue(parentPath, out GameplayTagInfoView parentView))
                {
                    parentView.AddContent(gameplayTagInfoView);
                }
                else
                {
                    AddMissingGameplayTagInfoView(parentPath).AddContent(gameplayTagInfoView);
                }
            }
            m_GameplayTagInfoViewMap.Add(tag, gameplayTagInfoView);
            return gameplayTagInfoView;
        }
        GameplayTagInfoView AddRuntimeGameplayTagInfoView(string tag)
        {
            var splitStrings = tag.Split('.');
            string lastPath = splitStrings[splitStrings.Length - 1];

            GameplayTagInfoView gameplayTagInfoView = new GameplayTagInfoView("(Runtime)" +lastPath, tag, m_TagContainer.ContainsTag(tag));
            gameplayTagInfoView.Top.AddToClassList("runtime");
            gameplayTagInfoView.Expanded = true;
            gameplayTagInfoView.OnToggled = (i) =>
            {
                string tag = m_GameplayTagInfoViewMap.Reverse[gameplayTagInfoView];

                if (i.newValue)
                {
                    m_UndoHelper.Do(() =>
                    {
                        if (!m_TagData[tag].Multi)
                        {
                            var parentTags = GameplayTagUtility.GetParentTags(tag);
                            for (int i = parentTags.Length - 1; i >= 0; i--)
                            {
                                if (m_TagData[parentTags[i]].Multi)
                                    break;
                                else
                                    m_TagContainer.RemoveTagWithChildRuntime(parentTags[i]);
                            }
                        }
                        m_TagContainer.AddTag(m_TagData.NameToInfo(tag));
                    }, "Select Tag");
                }
                else
                {
                    m_UndoHelper.Do(() =>
                    {
                        m_TagContainer.RemoveTagWithChildRuntime(tag);
                        var parentTags = GameplayTagUtility.GetParentTags(tag);
                        foreach (var parentTag in parentTags)
                        {
                            m_TagContainer.AddTagRuntime(parentTag);
                        }
                    }, "Unselect Tag");
                }
            };


            if (splitStrings.Length == 1)
            {
                m_Container.Add(gameplayTagInfoView);
                gameplayTagInfoView.AddToClassList("rootFolder");
            }
            else
            {
                string parentPath = tag.Substring(0, Mathf.Max(tag.Length - lastPath.Length - 1, 0));
                if (m_GameplayTagInfoViewMap.TryGetValue(parentPath, out GameplayTagInfoView parentView))
                    parentView.AddContent(gameplayTagInfoView);
                else
                    AddRuntimeGameplayTagInfoView(parentPath).AddContent(gameplayTagInfoView);
            }
            m_GameplayTagInfoViewMap.Add(tag, gameplayTagInfoView);
            return gameplayTagInfoView;
        }

        void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
        {
            Close();
        }
        void UndoRedo()
        {
            Focus();
            if (!Application.isPlaying)
            {
                m_TagContainer.Init();
                m_TagData.Init();
            }
        }

        bool inited;
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            rootVisualElement.schedule.Execute(() =>
            {
                if (!inited)
                {
                    if (rootVisualElement.worldBound.width <= m_Container.worldBound.width)
                        position = new Rect(position.position, new Vector2(m_Container.worldBound.width, position.height));

                    minSize = new Vector2(m_Container.worldBound.width + 50, position.height);
                    inited = true;
                }
            });
        }

        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            if (HasOpenInstances<GameplayTagSelectWindow>())
                GetWindow<GameplayTagSelectWindow>().Close();
        }
    }
}