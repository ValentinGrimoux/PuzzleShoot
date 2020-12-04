using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Utils.SuperCloner {

    [ExecuteInEditMode]
    public class WrapWithACloner : MonoBehaviour {

        [MenuItem("Tools/SuperCloner/Wrap")]
        public static void WrapSelectionWithACloner() {
            if (Selection.activeTransform != null) {
                Selection.activeTransform = WrapTargetWithACloner(Selection.activeTransform);
            }
        }

        public static Transform WrapTargetWithACloner(Transform target) {

            // if (target.parent == null && IsInPrefabMode) ...

            var go = new GameObject(string.Format("SuperCloner: {0}", target.gameObject.name));
            go.transform.position = target.position;
            go.transform.rotation = target.rotation;
            go.transform.SetParent(target.parent);
            go.transform.SetSiblingIndex(target.GetSiblingIndex());

            target.SetParent(go.transform, true);
            target.gameObject.SetActive(false);

            var cloner = go.AddComponent<SuperCloner>();
            cloner.AddSource(target.gameObject);
            cloner.AddLinearOperator();
            cloner.ComputeAll();

            return go.transform;
        }

        void Start() {
            var target = transform;
            DestroyImmediate(this);
            EditorApplication.delayCall += () => {
                Selection.activeTransform = WrapTargetWithACloner(target);
            };
        }

    }
}
#endif
