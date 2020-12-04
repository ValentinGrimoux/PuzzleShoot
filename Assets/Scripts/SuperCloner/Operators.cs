using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils.SuperCloner {

    public enum IterationMode {
        STEP,
        BOUND,
    }

    public abstract class Operator {

        public static System.Type[] types = {
            typeof(LinearOperator),
            typeof(RadialOperator),
            typeof(GridOperator),
        };

        [HideInInspector]
        public bool enabled = true;

        [HideInInspector, System.NonSerialized]
        public int arrayIndex;

        [HideInInspector]
        public int operatorIndex;

        public abstract int GetCount();
        public virtual Matrix4x4 GetMatrix(int index) => Matrix4x4.identity;
    }

    [System.Serializable]
    public class LinearOperator : Operator {

        [Range(1, 20)]
        public int count = 3;

        [Range(-1, 1)]
        public float align = 0;
        public IterationMode mode = IterationMode.STEP;

        public Vector3 movement = Vector3.right;
        public Vector3 rotation = Vector3.zero;

        public override int GetCount() => count;

        public override Matrix4x4 GetMatrix(int index) {

            float t = (float)index;
            
            t += -(count - 1f) * (0.5f - align / 2f);
            
            if (mode == IterationMode.BOUND) {
                if (count > 0) {
                    t /= (count - 1);
                }
            }

            return Matrix4x4.TRS(movement * t, Quaternion.Euler(rotation * t), Vector3.one);
        }

        public LinearOperator(int arrayIndex, int operatorIndex) {
            this.arrayIndex = arrayIndex;
            this.operatorIndex = operatorIndex;
        }
    }

    [System.Serializable]
    public class RadialOperator : Operator {

        public enum Axis {
            X, Y, Z,
        }

        [Range(1, 20)]
        public int count = 5;

        public float radius = 3f;
        public Axis axis = Axis.Z;

        [Range(0, 360)]
        public float arc = 360f;

        [Range(0, 360)]
        public float offset = 0f;

        public override int GetCount() => count;
        public override Matrix4x4 GetMatrix(int index) {

            float t = (float)index / count;
            float a = (t + offset / 360f) * (arc / 360f) * 2f * Mathf.PI;
            float x = radius * Mathf.Cos(a);
            float y = radius * Mathf.Sin(a);

            if (axis == Axis.Z) {
                return Matrix4x4.TRS(new Vector3(x, y, 0), Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg), Vector3.one);
            } else if (axis == Axis.Y) {
                return Matrix4x4.TRS(new Vector3(x, 0, y), Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f), Vector3.one);
            } else {
                return Matrix4x4.TRS(new Vector3(0, y, x), Quaternion.Euler(-a * Mathf.Rad2Deg, 0f, 0f), Vector3.one);
            }
        }

        public RadialOperator(int arrayIndex, int operatorIndex) {
            this.arrayIndex = arrayIndex;
            this.operatorIndex = operatorIndex;
        }
    }

    [System.Serializable]
    public class GridOperator : Operator {

        [Range(1, 20)]
        public int countX = 2, countY = 2, countZ = 2;

        [Range(-1, 1)]
        public float alignX = 0;
        [Range(-1, 1)]
        public float alignY = 0;
        [Range(-1, 1)]
        public float alignZ = 0;

        public IterationMode mode = IterationMode.STEP;

        public Vector3 movement = Vector3.one;

        public override int GetCount() => countX * countY * countZ;
        public override Matrix4x4 GetMatrix(int index) {
            
            int countXY = countX * countY;
            int z = index / countXY;
            index -= z * countXY;
            int y = index / countX;
            int x = index - y * countX;
            
            Vector3 t = new Vector3(x, y, z);
            Vector3 align = new Vector3(alignX, alignY, alignZ);
            Vector3 count = new Vector3(countX, countY, countZ);

            t += -Vector3.Scale(count - Vector3.one, new Vector3(0.5f, 0.5f, 0.5f) - align / 2f);

            if (mode == IterationMode.BOUND) {
                if (countX > 1) {
                    t.x /= countX - 1;
                }
                if (countY > 1) {
                    t.y /= countY - 1;
                }
                if (countZ > 1) {
                    t.z /= countZ - 1;
                }
            }

            return Matrix4x4.Translate(Vector3.Scale(movement, t));
        }

        public GridOperator(int arrayIndex, int operatorIndex) {
            this.arrayIndex = arrayIndex;
            this.operatorIndex = operatorIndex;
        }
    }

}