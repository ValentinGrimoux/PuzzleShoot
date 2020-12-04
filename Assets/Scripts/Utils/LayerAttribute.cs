using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils {

    public class LayerAttribute : PropertyAttribute {

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(LayerAttribute))]
        public class LayerAttributeDrawer : PropertyDrawer {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
        }
#endif
    }

    public class LayerMaskAttribute : PropertyAttribute {

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(LayerMaskAttribute))]
        public class MaskAttributeDrawer : PropertyDrawer {
            string[] names;
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                if (names == null) {
                    names = Enumerable.Range(0, 32).Select(i => LayerMask.LayerToName(i)).ToArray();
                }
                property.intValue = EditorGUI.MaskField(position, label, property.intValue, names);
            }
        }
#endif
    }
}
