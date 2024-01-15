using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public class GameplayTagContainerView : VisualElement
    {
        Label m_FieldName;
        public Label FieldName => m_FieldName;

        ScrollView m_ScrollView;
        public ScrollView ScrollView => m_ScrollView;

        Label m_TagContainer;
        public Label TagContainer => m_TagContainer;

        Button m_SelectButton;
        public Button SelectButton => m_SelectButton;

        GameplayTagContainer m_GameplayTagContainer;
        Object m_Owner;

        private VisualElement m_CachedInspectorElement;
        private VisualElement m_CachedContextWidthElement;

        private float m_LabelWidthRatio;
        private float m_LabelExtraPadding;
        private float m_LabelBaseMinWidth;
        private float m_LabelExtraContextWidth; 

        public static readonly string ussClassName = "unity-base-field";
        private static readonly string inspectorFieldUssClassName = ussClassName + "__inspector-field";

        public GameplayTagContainerView(string fieldName, GameplayTagContainer gameplayTagContainer, Object owner)
        {
            var visualTree = Resources.Load<VisualTreeAsset>("VisualTree/GameplayTagContainer");
            visualTree.CloneTree(this);
            
            AddToClassList("gameplayTagContainerView");

            m_FieldName = this.Q<Label>("field-name");
            m_FieldName.text = fieldName;

            m_ScrollView = this.Q<ScrollView>();          
            m_TagContainer = this.Q<Label>("tag-container");           
            m_SelectButton = this.Q<Button>("select-tag-button");

            m_GameplayTagContainer = gameplayTagContainer;
            m_GameplayTagContainer.ReferencePath = $"{GlobalObjectId.GetGlobalObjectIdSlow(owner)}/{fieldName}";
            if (!Application.isPlaying)
                m_GameplayTagContainer.Init();

            m_Owner = owner;

            m_SelectButton.clicked += () =>
            {
                m_SelectButton.SetEnabled(false);

                GameplayTagEditorUtility.GameplayTagContainer = m_GameplayTagContainer;
                GameplayTagEditorUtility.GameplayTagContainerOwner = m_Owner;
                var window = EditorWindow.GetWindow<GameplayTagSelectWindow>();
                window.Show();

                Rect rect = new Rect();
                rect.xMin = GUIUtility.GUIToScreenRect(m_ScrollView.worldBound).xMin;
                rect.yMin = GUIUtility.GUIToScreenRect(m_ScrollView.worldBound).yMin + m_ScrollView.worldBound.height + 10;
                rect.width = 200;
                rect.height = 400;
                window.position = rect;
                
                lastHeight = m_ScrollView.worldBound.height;
                lastRect = rect;
                window.OnClosed += () => EditorCoroutineHelper.Delay(() => m_SelectButton.SetEnabled(true), 0.02f);
            };

            PopulateView();


            GameplayTagUtility.GameplayTagData.OnValueChanged += m_GameplayTagContainer.Init;
            m_GameplayTagContainer.OnValueChanged += PopulateView;
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDestroy);
        }

        void PopulateView()
        {          
            m_TagContainer.Clear();
            m_TagContainer.text = "Empty";
            foreach (var tag in m_GameplayTagContainer.Tags)
            {
                if (!m_GameplayTagContainer.HasChildTag(tag))
                {
                    m_TagContainer.Add(new Label(tag));
                    m_TagContainer.text = string.Empty;
                }
            }
        }

        void OnDestroy(DetachFromPanelEvent e)
        {
            GameplayTagUtility.GameplayTagData.OnValueChanged -= m_GameplayTagContainer.Init;
            m_GameplayTagContainer.OnValueChanged -= PopulateView;
        }

        float lastHeight;
        Rect lastRect;
        void UpdatePosition()
        {
            if(EditorWindow.focusedWindow is GameplayTagSelectWindow gameplayTagSelectWindow)
            {
                float deltaHeight = m_ScrollView.worldBound.height - lastHeight;
                if (Mathf.Approximately(deltaHeight, 0))
                    return;

                lastHeight = m_ScrollView.worldBound.height;
                lastRect = new Rect(new Vector2(lastRect.xMin, lastRect.yMin + deltaHeight), lastRect.size);
                
                if (lastRect.position.x == 0)
                    return;
                gameplayTagSelectWindow.position = lastRect;
            }
        }
        void OnAttachToPanel(AttachToPanelEvent e)
        {
            if (e.destinationPanel == null || e.destinationPanel.contextType == ContextType.Player)
            {
                return;
            }

            for (VisualElement visualElement = base.parent; visualElement != null; visualElement = visualElement.parent)
            {
                if (visualElement.ClassListContains("unity-inspector-element"))
                {
                    m_CachedInspectorElement = visualElement;
                }

                if (visualElement.ClassListContains("unity-inspector-main-container"))
                {
                    m_CachedContextWidthElement = visualElement;
                    break;
                }
            }

            if (m_CachedInspectorElement != null)
            {
                m_LabelWidthRatio = 0.45f;
                m_LabelExtraPadding = 37f;
                m_LabelBaseMinWidth = 123;
                m_LabelExtraContextWidth = 1f;
                AddToClassList(inspectorFieldUssClassName);
                RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
            }
        }
        void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
        {
            AlignLabel();
            UpdatePosition();
        }
        void AlignLabel()
        {
            float labelExtraPadding = m_LabelExtraPadding;
            float num = base.worldBound.x - m_CachedInspectorElement.worldBound.x - m_CachedInspectorElement.resolvedStyle.paddingLeft;
            labelExtraPadding += num;
            labelExtraPadding += base.resolvedStyle.paddingLeft;
            float a = m_LabelBaseMinWidth - num - base.resolvedStyle.paddingLeft;
            VisualElement visualElement = m_CachedContextWidthElement ?? m_CachedInspectorElement;
            m_FieldName.style.minWidth = Mathf.Max(a, 0f);
            float num2 = (visualElement.resolvedStyle.width + m_LabelExtraContextWidth) * m_LabelWidthRatio - labelExtraPadding;
            if (Mathf.Abs(m_FieldName.resolvedStyle.width - num2) > 1E-30f)
            {
                m_FieldName.style.width = Mathf.Max(0f, num2);
            }
        }
    }
}