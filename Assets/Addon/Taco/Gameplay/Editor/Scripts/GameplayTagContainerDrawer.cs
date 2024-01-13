using UnityEngine.UIElements;
using UnityEditor;

namespace Taco.Gameplay.Editor
{
    [CustomPropertyDrawer(typeof(GameplayTagContainer))]
    public class GameplayTagContainerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            GameplayTagContainerView gameplayTagContainerView = new GameplayTagContainerView(property.displayName, fieldInfo.GetValue(property.serializedObject.targetObject) as GameplayTagContainer, property.serializedObject.targetObject);            
            return gameplayTagContainerView;
        }
    }
}