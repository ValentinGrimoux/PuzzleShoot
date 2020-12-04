#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using static UnityEditor.EditorGUILayout;

namespace Utils.SuperCloner {

    public static class Inspector {

        static GUIStyle title = new GUIStyle(GUI.skin.label);
        static GUIStyle box = new GUIStyle(EditorStyles.helpBox);
        
        static Inspector() {
            title.fontSize = 24;
            title.fixedHeight = 32;
            title.stretchWidth = false;

            box.padding = new RectOffset(8, 8, 8, 8);
        }

        static void Title(string label) {
            LabelField(label, title);
            Space(8);
        }

        static bool Button(string label) => GUILayout.Button(label);
        static void LabelFieldFormat(string label, params object[] insertions) => LabelField(string.Format(label, insertions));

        public static void Draw(SuperCloner cloner, SerializedObject so, Clone clone = null) {

            string title = clone == null ? "SuperCloner" : string.Format("SuperCloner Clone#{0}", clone.index);
            Title(title);

            LabelFieldFormat("{0} operators, {1} matrices, {2}/{3} clones", 
                cloner.operatorCount,
                cloner.matrices.Length,
                cloner.clones.Count,
                cloner.IdealCloneCount);
            Space(16);

            bool needComputeMatrices = false;
            
            PropertyField(so.FindProperty("reversedDimensions"));
            if (so.hasModifiedProperties) {
                so.ApplyModifiedProperties();
                needComputeMatrices = true;
            }

            PropertyField(so.FindProperty("hideClonerInHierarchy"));
            if (so.hasModifiedProperties) {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(cloner);
                cloner.NeverHideBoth(false);
            }

            PropertyField(so.FindProperty("hideClonesInHierarchy"));
            if (so.hasModifiedProperties) {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(cloner);
                cloner.NeverHideBoth(true);
            }

            GUI.enabled = clone == null;
            PropertyField(so.FindProperty("dryRun"));
            if (so.hasModifiedProperties) {
                so.ApplyModifiedProperties();
                cloner.ComputeClones();
                EditorUtility.SetDirty(cloner);
            }
            GUI.enabled = true;

            PropertyField(so.FindProperty("sources"));
            if (Button("Use Children As Sources")) {
                cloner.UseChildrenAsSources();
                cloner.ComputeClones();
                EditorUtility.SetDirty(cloner);
            }
            if (Button("Regenerate Clones")) {
                cloner.DestroyClones();
                cloner.ComputeClones();
                return;
            }
            if (Button("Destroy Cloner And Release Clones")) {
                Selection.objects = cloner.DestroyClonerAndReleaseClones();
                return;
            }
            Space(16);

            var linearOperators = so.FindProperty("linearOperators");
            if (Button("Add Linear Operator")) {
                linearOperators.arraySize++;
                so.ApplyModifiedProperties();
                cloner.LastLinearOperator = new LinearOperator(cloner.linearOperators.Length - 1, cloner.operatorCount++);
                cloner.LastLinearOperator.movement = cloner.GetLastNewLinearOperator();
                needComputeMatrices = true;
            }

            var radialOperators = so.FindProperty("radialOperators");
            if (Button("Add Radial Operator")) {
                radialOperators.arraySize++;
                so.ApplyModifiedProperties();
                cloner.LastRadialOperator = new RadialOperator(cloner.radialOperators.Length - 1, cloner.operatorCount++);
                needComputeMatrices = true;
            }

            var gridOperators = so.FindProperty("gridOperators");
            if (Button("Add Grid Operator")) {
                gridOperators.arraySize++;
                so.ApplyModifiedProperties();
                cloner.LastGridOperator = new GridOperator(cloner.gridOperators.Length - 1, cloner.operatorCount++);
                needComputeMatrices = true;
            }

            Space(16);

            var operators = cloner.ComputeOperators();
            var soOperators = new SerializedProperty[] { linearOperators, radialOperators, gridOperators };
            for (int index = 0; index < operators.Length; index++) {
                var op = operators[index];
                var opType = op.GetType();
                BeginVertical(box);
                
                BeginHorizontal();
                LabelFieldFormat("{0} — {1}", index, opType.Name);
                Space(0, true);
                bool newEnabled = Toggle(op.enabled, GUILayout.Width(16));
                if (newEnabled != op.enabled) {
                    op.enabled = newEnabled;
                    EditorUtility.SetDirty(cloner);
                    needComputeMatrices = true;
                }
                EndHorizontal();
                Space(8);

                var soOperator = soOperators[System.Array.IndexOf(Operator.types, opType)];
                foreach(SerializedProperty p in soOperator.GetArrayElementAtIndex(op.arrayIndex)) {
                    // do not draw subproperty (egsoOperator: "X" from "Movement")
                    if (p.depth > 2) continue; 
                    EditorGUILayout.PropertyField(p);
                }
                Space(8);
                BeginHorizontal();
                int newIndex = Popup(index, Enumerable.Range(0, operators.Length).Select(i => i.ToString()).ToArray());
                if (newIndex != index) {
                    operators[index].operatorIndex = newIndex;
                    operators[newIndex].operatorIndex = index;
                    cloner.RebuildOperatorIndexes();
                    EditorUtility.SetDirty(cloner);
                    return; // NOTE: Important to return here if something has been modified
                }
                GUI.enabled = index > 0;
                if (Button("Move Up")) {
                    operators[index].operatorIndex--;
                    operators[index - 1].operatorIndex++;
                    cloner.RebuildOperatorIndexes();
                    EditorUtility.SetDirty(cloner);
                    return; // NOTE: Important to return here if something has been modified
                }
                GUI.enabled = index < operators.Length - 1;
                if (Button("Move Down")) {
                    operators[index].operatorIndex++;
                    operators[index + 1].operatorIndex--;
                    cloner.RebuildOperatorIndexes();
                    EditorUtility.SetDirty(cloner);
                    return; // NOTE: Important to return here if something has been modified
                }
                GUI.enabled = true;
                if (Button("Remove")) {
                    soOperator.DeleteArrayElementAtIndex(op.arrayIndex);
                    so.ApplyModifiedProperties();
                    cloner.RebuildOperatorIndexes();
                    return; // NOTE: Important to return here if something has been modified
                }
                EndHorizontal();
                EndVertical();
            }

            if (so.hasModifiedProperties) {
                needComputeMatrices = true;
            }

            Space(16);
            bool newGizmoShowOptions = Foldout(cloner.gizmoShowOptions, "Gizmo Options");
            if (newGizmoShowOptions != cloner.gizmoShowOptions) {
                cloner.gizmoShowOptions = newGizmoShowOptions;
                so.Update();
            }
            if (newGizmoShowOptions) {
                PropertyField(so.FindProperty("gizmoShowPoints"));
                PropertyField(so.FindProperty("gizmoShowAxis"));
                PropertyField(so.FindProperty("gizmoShowIndexes"));
                PropertyField(so.FindProperty("gizmoColorA"));
                PropertyField(so.FindProperty("gizmoColorB"));
                PropertyField(so.FindProperty("gizmoSortByDistance"));
            }

            Space(16);
            bool newShowDebug = Foldout(cloner.showDebug, "Debug");
            if (newShowDebug != cloner.showDebug) {
                cloner.showDebug = newShowDebug;
                so.Update();
            }
            if (newShowDebug) {
                Debug(cloner);
            }

            so.ApplyModifiedProperties();

            if (needComputeMatrices) {
                cloner.ComputeMatrices();
            }
        }

        public static void Debug(SuperCloner cloner) {

            LabelField(SuperCloner.versionString);
            LabelFieldFormat("{0} operators ({2}/{1})", cloner.operatorCount, cloner.operators.Length, cloner.GetEnabledOperators().Length);
            LabelFieldFormat("{0} matrices, {1} clones", cloner.matrices.Length, cloner.clones.Count);
            LabelFieldFormat("ComputeMatrices {0}ms", cloner.ComputeMatricesDuration);
            
            if (Button("Force ComputeMatrices()")) {
                cloner.ComputeMatrices();
            }

            if (Button("Force ComputeClones()")) {
                cloner.ComputeClones();
            }

            if (Button("Destroy And Regenerate Clones")) {
                cloner.DestroyClones();
                cloner.ComputeClones();
            }

            if (Button("HideFlags: Show Any Hidden Clones")) {
                foreach(var clone in GameObject.FindObjectsOfType<Clone>()) {
                    clone.gameObject.hideFlags = HideFlags.None;
                }
            }

            if (Button("HideFlags: Restore Clones Hideflags")) {
                foreach(var cloner2 in GameObject.FindObjectsOfType<SuperCloner>()) {
                    cloner2.ArrangeClones(false);
                    EditorUtility.SetDirty(cloner2);
                }
            }
        }
    }
}
#endif