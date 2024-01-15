using System;
using UnityEngine;
using UnityEngine.UIElements;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public class GameplayTagInfoView : FolderView
    {
        Toggle m_ToggleButton;
        public Toggle ToggleButton => m_ToggleButton;

        Label m_MultiLabel;
        public Label MultiLabel => m_MultiLabel;

        Button m_RepairButton;
        public Button RepairButton => m_RepairButton;


        public Action<ChangeEvent<bool>> OnToggled;

        const string m_VisualTreeAssetGUID = "3d4f51df3af732645b926124dc20e19d";

        public GameplayTagInfoView(string shortTag, string detailTag, bool toggled) : base(m_VisualTreeAssetGUID, shortTag, detailTag, null)
        {
            m_ToggleButton = this.Q<Toggle>("toggle-button");
            m_ToggleButton.value = toggled;
            m_ToggleButton.RegisterValueChangedCallback(i => OnToggled?.Invoke(i));

            m_MultiLabel = this.Q<Label>("multi-label");
            if (GameplayTagEditorUtility.GameplayTagData.Contains(detailTag))
                m_MultiLabel.text = GameplayTagEditorUtility.GameplayTagData[detailTag].Multi ? "Multi" : "Single";

            m_OptionButton.AddManipulator(new DropdownMenuManipulator((menu) =>
            {
                menu.AppendAction("Copy shortTag name", (a) =>
                {
                    GUIUtility.systemCopyBuffer = detailTag;
                });

                if (GameplayTagEditorUtility.GameplayTagData[detailTag].Multi)
                {
                    menu.AppendAction("Turn to single", (a) =>
                    {
                        GameplayTagEditorUtility.SetMultiState(detailTag, false);
                        m_MultiLabel.text = "Single";
                    });
                }
                else
                {
                    menu.AppendAction("Turn to multi", (a) =>
                    {
                        GameplayTagEditorUtility.SetMultiState(detailTag, true);
                        m_MultiLabel.text = "Multi";
                    });
                }
            }, MouseButton.LeftMouse));
            m_OptionButton.AddToClassList("optional");

            m_RepairButton = this.Q<Button>("repair-buttton");
        }
    }
}