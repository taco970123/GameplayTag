using System;
using UnityEditor;

namespace Taco.Editor
{
    public class UndoHelper
    {
        public string GroupName;
        public UnityEngine.Object Owner;
        public event Action OnUndoCallback;
        
        public string LastUndoName;

        public UndoHelper(string groupName, UnityEngine.Object owner, Action onUndoCallback)
        {
            GroupName = groupName;
            Owner = owner;
            OnUndoCallback = onUndoCallback;
            Undo.undoRedoEvent += OnUndoRedoEvent;
        }
        public void Dispose()
        {
            Undo.ClearUndo(Owner);
            Undo.undoRedoEvent -= OnUndoRedoEvent;
        }



        public void Do(Action action,string actionName)
        {
            Undo.RegisterCompleteObjectUndo(Owner, $"{GroupName}: {actionName}");
            action?.Invoke();
            EditorUtility.SetDirty(Owner);
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

        public static void Do(UnityEngine.Object owner, Action action, string actionName)
        {
            Undo.RegisterCompleteObjectUndo(owner, actionName);
            action?.Invoke();
            EditorUtility.SetDirty(owner);
        }
    }
}