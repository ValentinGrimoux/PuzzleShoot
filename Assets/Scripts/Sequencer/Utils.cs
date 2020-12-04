using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sequencer {

    public static class Utils {
        
        public static Mesh disc = new Mesh();

        static Utils() {
            int step = 32;
            float radius = 0.5f;
            var vertices = new Vector3[step + 2];
            var triangles = new int[step * 3 * 2]; // x 2 because backfaces
            int backfaceOffset = step * 3;

            for (int index = 0; index <= step; index++) {
                float angle = Mathf.PI * 2f * (float)index / (float)step;
                vertices[index + 1] = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0f);
                if (index < step) {
                    // FrontFace
                    triangles[index * 3 + 0] = 0;
                    triangles[index * 3 + 1] = 1 + index + 1;
                    triangles[index * 3 + 2] = 1 + index;
                    // BackFace
                    triangles[backfaceOffset + index * 3 + 0] = 0;
                    triangles[backfaceOffset + index * 3 + 1] = 1 + index;
                    triangles[backfaceOffset + index * 3 + 2] = 1 + index + 1;
                }
            }
            disc.vertices = vertices;
            disc.triangles = triangles;
            disc.normals = Enumerable.Repeat(Vector3.back, step + 2).ToArray();
        }

        public static IEnumerable<(Vector3, Vector3)> ChordAround(Vector3 center, float radius, int step = 32) {
            Vector3 A = center + Vector3.right * radius;
            for (int index = 0; index < step; index++) {
                float angle = Mathf.PI * 2f * ((float)index + 1f) / (float)step;
                Vector3 B = new Vector3(center.x + radius * Mathf.Cos(angle), center.y + radius * Mathf.Sin(angle), center.z);
                yield return (A, B);
                A = B;
            }
        }

        public static string CapitalsOnly(string label) {
            int index = 0;
            int max = label.Length;
            List<char> chars = new List<char>();
            bool nextCharMustBeAdd = true;
            while (index < max) {
                char c = label[index++];
                if (nextCharMustBeAdd) {
                    chars.Add(c);
                    nextCharMustBeAdd = false;
                }
                else if (char.IsUpper(c)) {
                    chars.Add(c);
                }
                else if (char.IsWhiteSpace(c)) {
                    nextCharMustBeAdd = true;
                }
            }
            return new string(chars.ToArray());
        }

#if UNITY_EDITOR
        public static bool GetSelected(Transform scope) {

            while (scope != null) {
                if (Selection.gameObjects.Contains(scope.gameObject)) {
                    return true;
                }
                scope = scope.parent;
            }
            return false;
        }
#endif
    }
}