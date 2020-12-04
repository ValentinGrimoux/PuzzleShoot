using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils.SuperCloner {

    public partial class SuperCloner : MonoBehaviour {

        public bool gizmoShowOptions = false;
        public bool gizmoShowPoints = false;
        public bool gizmoShowAxis = false;
        public bool gizmoShowIndexes = false;
        public Color gizmoColorA = new Color(1f, 0.7f, 0.7f);
        public Color gizmoColorB = new Color(1f, 0.1f, 0.1f);
        public bool gizmoSortByDistance = true;

#if UNITY_EDITOR
        void OnDrawGizmos() {

            if (!enabled) return;

            PainfulCallOfOnDrawGizmosOnClonesComponents();

            bool showPoints = gizmoShowPoints || (clones.Count == 0 && matrices.Length > 0);

            if (!showPoints && !gizmoShowAxis && !gizmoShowIndexes) return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;
            
            var cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
            var ms = gizmoSortByDistance 
                ? GetSortedByDistanceMatrices(cameraPosition).Select(b => (b.matrix, b.index))
                : matrices.Select((m, i) => (m, i));

            GUIStyle style = new GUIStyle();
            
            foreach(var (m, index) in ms) {

                var p = GeomUtils.ExtractPosition(m);

                if (showPoints || gizmoShowIndexes) {
                    float x = matrices.Length > 1 ? (float)index / (matrices.Length - 1) : 0f;
                    Gizmos.color = Color.Lerp(gizmoColorA, gizmoColorB, x);
                }
                
                if (showPoints) {
                    Gizmos.DrawSphere(p, 0.1f);
                }

                if (gizmoShowIndexes) {
                    style.normal.textColor = Gizmos.color;
                    Handles.Label(p, string.Format("{0}", index), style);
                }

                if (gizmoShowAxis) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(p, p + GeomUtils.ExtractRight(m) * 0.5f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(p, p + GeomUtils.ExtractUp(m) * 0.5f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(p, p + GeomUtils.ExtractForward(m) * 0.5f);
                }
            }
        }
        

        void PainfulCallOfOnDrawGizmosOnClonesComponents() {

            // Because of a very annoying bug ('OnDrawGizmos' not being called on hidden gameObjects),
            // we need to use Reflection to invoke "OnDrawGizmos".

            Gizmos.matrix = Matrix4x4.identity;
            foreach(var clone in clones) {
                foreach(var component in clone.GetComponents<Component>()) {
                    MethodInfo mi = component.GetType().GetMethod("OnDrawGizmos", BindingFlags.NonPublic | BindingFlags.Instance);
                    mi?.Invoke(component, new object[0]);
                }
            }
        }
#endif
    }
}