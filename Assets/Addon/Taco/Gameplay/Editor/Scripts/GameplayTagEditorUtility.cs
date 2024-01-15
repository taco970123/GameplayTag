using System.IO;
using UnityEngine;
using UnityEditor;
using Taco.Editor;

namespace Taco.Gameplay.Editor
{
    public static class GameplayTagEditorUtility
    {
        const string DataPath = "Assets/Resources";
        public static GameplayTagData GameplayTagData
        {
            get
            {
                if (GameplayTagUtility.GameplayTagData == null)
                    Init();
                return GameplayTagUtility.GameplayTagData;
            }
        }

        public static GameplayTagContainer GameplayTagContainer;
        public static Object GameplayTagContainerOwner;


        static int s_UndoListener;
        public static UndoHelper DataUndoHelper { get; private set; }

        static void Init()
        {
            GameplayTagUtility.GameplayTagData = Resources.Load<GameplayTagData>("GameplayTagData");
            if (GameplayTagUtility.GameplayTagData == null)
            {
                if (!Directory.Exists(DataPath))
                    Directory.CreateDirectory(DataPath);

                GameplayTagUtility.GameplayTagData = ScriptableObject.CreateInstance<GameplayTagData>();
                AssetDatabase.CreateAsset(GameplayTagData, $"{DataPath}/GameplayTagData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            GameplayTagUtility.GameplayTagData.Init();
        }

        public static void RegisterUndo(System.Action onUndoCallback)
        {
            if (s_UndoListener == 0)
                DataUndoHelper = new UndoHelper("GameplayTagEditData", OnUndo, GameplayTagData);

            s_UndoListener++;
            if (onUndoCallback != null)
                DataUndoHelper.OnUndoCallback += onUndoCallback;
        }
        public static void UnregisterUndo(System.Action onUndoCallback)
        {
            if (DataUndoHelper == null)
                return;

            s_UndoListener--;
            if (onUndoCallback != null)
                DataUndoHelper.OnUndoCallback -= onUndoCallback;

            if (s_UndoListener == 0)
                DataUndoHelper.Dispose();
        }
        static void OnUndo()
        {
            GameplayTagData.Init();
        }


        public static void SetExpandedState(string name, bool state)
        {
            GameplayTagData.SetExpandedState(name, state);
        }
        public static void SetMultiState(string name, bool state)
        {
            DataUndoHelper.Do(() => GameplayTagData.SetMultiState(name, state), "Change Multi State");
        }
    }
}