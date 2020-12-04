using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils.SuperCloner {

    [ExecuteAlways]
    public class Clone : MonoBehaviour {
        public SuperCloner cloner;
        public GameObject source;
        public int index;

        void Update() {
            if (cloner == null) {
                gameObject.hideFlags = HideFlags.None;
                DestroyImmediate(this);
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Clone))]
        class MyEditor : Editor {

            SerializedObject soCloner;
            void OnEnable() => soCloner = new SerializedObject(clone.cloner);

            Clone clone => target as Clone;
            public override void OnInspectorGUI() {

                GUI.enabled = false;
                base.OnInspectorGUI();
                GUI.enabled = true;

                EditorGUILayout.Space(16);
                Inspector.Draw(clone.cloner, soCloner, clone);
            }
        }
#endif
    }
}