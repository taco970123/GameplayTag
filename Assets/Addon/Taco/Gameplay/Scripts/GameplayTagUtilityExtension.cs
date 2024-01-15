#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Taco.Gameplay
{
    public static partial class GameplayTagUtility
    {
        [InitializeOnLoadMethod]
        public static void EditorInit()
        {
            GameplayTagData = Resources.Load<GameplayTagData>("GameplayTagData");
            GameplayTagData?.Init();
        }
    }
}
#endif