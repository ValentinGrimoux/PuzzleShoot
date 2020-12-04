using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Utils {

    public static class DebugUtils {

        public static GameObject[] GetHiddenGameObjects() => 
            GameObject.FindObjectsOfType<Transform>()
            .Where(t => (t.gameObject.hideFlags & HideFlags.HideInHierarchy) > 0)
            .Select(t => t.gameObject)
            .ToArray();

        [MenuItem("Tools/Debug Utils/Unhide Any GameObjects")]
        public static void UnhideAnyGameObjects() {

            var hidden = GetHiddenGameObjects();
            Debug.LogFormat("Found {0} hidden GameObjects.", hidden.Length);
            
            foreach(var transform in hidden) {
                transform.gameObject.hideFlags = HideFlags.None;
            }
        }
    }
}
#endif
