using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sequencer {

    [System.Serializable]
    public class OptionColor {
        
        public bool active;
        public Color color = Color.red;

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(OptionColor))]
        class MyPropertyDrawer : PropertyDrawer {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

                EditorGUI.BeginProperty(position, label, property);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var activeWidth = 16;
                var activeRect = new Rect(position.x, position.y, activeWidth, position.height);
                var colorRect = new Rect(position.x + activeWidth, position.y, position.width - activeWidth, position.height);

                var activeProp = property.FindPropertyRelative("active");
                EditorGUI.PropertyField(activeRect, activeProp, GUIContent.none);
                if (activeProp.boolValue) {
                    EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("color"), GUIContent.none);
                }

                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}
