using System.Collections;
using Unity.EditorCoroutines.Editor;

namespace Taco.Editor
{ 
    public static class EditorCoroutineHelper
    {
        public static EditorCoroutine Delay(System.Action callback, float timer)
        {
            return EditorCoroutineUtility.StartCoroutineOwnerless(EditorCoroutine(callback, timer));
        }

        public static IEnumerator EditorCoroutine(System.Action callback, float timer)
        {
            yield return new EditorWaitForSeconds(timer);
            callback?.Invoke();
        }
    }
}