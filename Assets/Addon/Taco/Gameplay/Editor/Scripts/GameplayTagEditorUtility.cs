using UnityEngine;
using UnityEditor;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public static class GameplayTagEditorUtility
    {
        const string DefaultFolderGUID = "66b22ea9618232b48ae451c07e6594bb";

        static GameplayTagEditData s_GameplayTagEditData;
        public static GameplayTagEditData GameplayTagEditData
        {
            get
            {
                if(s_GameplayTagEditData == null)
                    Init();
                return s_GameplayTagEditData;
            }
        }

        public static GameplayTagContainer GameplayTagContainer;
        public static Object GameplayTagContainerOwner;


        static int s_UndoListener;
        public static UndoHelper EditorDataUndoHelper { get; private set; }

        static void Init()
        {
            string gameplayTagEditInfoPath = AssetDatabase.GUIDToAssetPath(DefaultFolderGUID) + "/GameplayTagEditData.asset";
            s_GameplayTagEditData = AssetDatabase.LoadAssetAtPath(gameplayTagEditInfoPath, typeof(GameplayTagEditData)) as GameplayTagEditData;
            if (s_GameplayTagEditData == null)
            {
                s_GameplayTagEditData = ScriptableObject.CreateInstance<GameplayTagEditData>();
                AssetDatabase.CreateAsset(s_GameplayTagEditData, gameplayTagEditInfoPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            s_GameplayTagEditData.Init();
        }

        public static void RegisterUndo(System.Action onUndoCallback)
        {
            if(s_UndoListener == 0)
                EditorDataUndoHelper = new UndoHelper("GameplayTagEditData", s_GameplayTagEditData, OnUndo);

            s_UndoListener++;
            if (onUndoCallback != null)
                EditorDataUndoHelper.OnUndoCallback += onUndoCallback;
        }
        public static void UnregisterUndo(System.Action onUndoCallback)
        {
            if (EditorDataUndoHelper == null)
                return;

            s_UndoListener--;
            if (onUndoCallback != null)
                EditorDataUndoHelper.OnUndoCallback -= onUndoCallback;

            if (s_UndoListener == 0)
                EditorDataUndoHelper.Dispose();
        }
        static void OnUndo()
        {
            s_GameplayTagEditData.Init();
        }


        public static void SetExpandedState(string name, bool state)
        {
            s_GameplayTagEditData.SetExpandedState(name, state);
        }
        public static void SetMultiState(string name, bool state)
        {
            EditorDataUndoHelper.Do(() => s_GameplayTagEditData.SetMultiState(name, state), "Change Multi State");
        }
    }
}