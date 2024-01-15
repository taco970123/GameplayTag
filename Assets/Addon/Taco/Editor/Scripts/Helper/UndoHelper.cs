using System;
using UnityEditor;

namespace Taco.Editor
{
    public class UndoHelper
    {
        public string GroupName;
        public UnityEngine.Object[] Owners;
        public event Action OnUndoCallback;
        
        public string LastUndoName;

        public UndoHelper(string groupName, Action onUndoCallback, params UnityEngine.Object[] owners)
        {
            GroupName = groupName;
            Owners = owners;
            OnUndoCallback = onUndoCallback;
            Undo.undoRedoEvent += OnUndoRedoEvent;
        }
        public void Dispose()
        {
            foreach (var owner in Owners)
            {
                Undo.ClearUndo(owner);
            }
            Undo.undoRedoEvent -= OnUndoRedoEvent;
        }
        public void Do(Action action,string actionName)
        {
            Undo.RegisterCompleteObjectUndo(Owners, $"{GroupName}: {actionName}");
            action?.Invoke();
            foreach (var owner in Owners)
            {
                EditorUtility.SetDirty(owner);
            }
        }

        bool IsThis()
        {
            return LastUndoName == GroupName;
        }

        void OnUndoRedoEvent(in UndoRedoInfo info)
        {
            LastUndoName = info.undoName.Split(':')[0];
            if(IsThis())
                OnUndoCallback?.Invoke();
        }

        public static void Do(Action action, string actionName,params UnityEngine.Object[] owners)
        {
            Undo.RegisterCompleteObjectUndo(owners, actionName);
            action?.Invoke();
            foreach (var owner in owners)
            {
                EditorUtility.SetDirty(owner);
            }
        }
    }
}