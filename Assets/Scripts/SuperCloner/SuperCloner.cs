using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils.SuperCloner {

    [ExecuteAlways]
    public partial class SuperCloner : MonoBehaviour {

        public static int[] version = { 0, 1, 0 };
        public static string versionString = string.Format("v{0}.{1}.{2}", version[0], version[1], version[2]);

        public int operatorCount = 0;

        public LinearOperator[] linearOperators = new LinearOperator[0];
        public LinearOperator LastLinearOperator {
            get => linearOperators.Last();
            set => linearOperators[linearOperators.Length - 1] = value;
        }
        public Vector3 GetLastNewLinearOperator() {
            var movements = new Vector3[]{
                Vector3.right * 3,
                Vector3.up * 3,
                Vector3.forward * 3,
            };
            return movements[(linearOperators.Length - 1) % 3];
        }
        public void AddLinearOperator(LinearOperator linearOperator = null) {
            linearOperators = linearOperators
                .Append(linearOperator ?? new LinearOperator(linearOperators.Length, operatorCount++))
                .ToArray();
        }

        public RadialOperator[] radialOperators = new RadialOperator[0];
        public RadialOperator LastRadialOperator {
            get => radialOperators.Last();
            set => radialOperators[radialOperators.Length - 1] = value;
        }

        public GridOperator[] gridOperators = new GridOperator[0];
        public GridOperator LastGridOperator {
            get => gridOperators.Last();
            set => gridOperators[gridOperators.Length - 1] = value;
        }

        public bool reversedDimensions = true;
        public bool hideClonerInHierarchy = false;
        public bool hideClonesInHierarchy = true;
        public bool dryRun = false;



        public Operator[][] GetOperatorArrays() {
            return new Operator[][] {
                linearOperators.Cast<Operator>().ToArray(),
                radialOperators.Cast<Operator>().ToArray(),
                gridOperators.Cast<Operator>().ToArray(),
            };
        }

        public void UpdateOperatorArrayIndex() {
           foreach(var operators in GetOperatorArrays()) {
                foreach(var (op, index) in operators.Select((x, y) => (x, y))) {
                    op.arrayIndex = index;
                }
            }
        }

        [System.NonSerialized]
        public Operator[] operators = new Operator[0];
        public Operator[] GetEnabledOperators() => operators.Where(op => op.enabled).ToArray();
        public Operator[] ComputeOperators() {
            UpdateOperatorArrayIndex();
            operators = 
                linearOperators.Cast<Operator>()
                .Concat(radialOperators.Cast<Operator>())
                .Concat(gridOperators.Cast<Operator>())
                .ToArray();
            System.Array.Sort(operators, (A, B) => A.operatorIndex < B.operatorIndex ? -1 : 1);
            return operators;
        }

        public void RebuildOperatorIndexes() {

            ComputeOperators();

            int operatorIndex = 0;
            int[] typesCount = new int[Operator.types.Length];
            foreach(var op in operators) {
                op.operatorIndex = operatorIndex++;
                op.arrayIndex = typesCount[System.Array.IndexOf(Operator.types, op.GetType())]++;
            }
            
            operatorCount = operatorIndex;

            System.Array.Sort(linearOperators, (A, B) => A.arrayIndex < B.arrayIndex ? -1 : 1);
            System.Array.Sort(radialOperators, (A, B) => A.arrayIndex < B.arrayIndex ? -1 : 1);
            System.Array.Sort(gridOperators, (A, B) => A.arrayIndex < B.arrayIndex ? -1 : 1);

            ComputeMatrices();
        }
        
        [System.NonSerialized]
        public Matrix4x4[] matrices = new Matrix4x4[0];
        public Matrix4x4[] GetMatrices() {

            var operators = GetEnabledOperators();

            if (operators.Length == 0) {
                return new Matrix4x4[0];
            }

            var dimensions = operators.Select(op => op.GetCount()).ToArray();
            int matriceCount = dimensions.Aggregate((x, y) => x * y);
            var matrices = Enumerable.Repeat(Matrix4x4.identity, matriceCount).ToArray();

            for (int opIndex = 0; opIndex < operators.Length; opIndex++) {
                var op = operators[opIndex];

                var indexesGenerator = reversedDimensions 
                    ? MathUtils.ForIndexesIn(dimensions.Reverse().ToArray(), operators.Count() - 1 - opIndex)
                    : MathUtils.ForIndexesIn(dimensions, opIndex);
                
                int dimIndex = 0;
                foreach(var indexes in indexesGenerator) {
                    var mat = op.GetMatrix(dimIndex);
                    foreach (var index in indexes) {
                        matrices[index] = mat * matrices[index];
                    }
                    dimIndex++;
                }
            }

            return matrices;
        }
        
        public long ComputeMatricesDuration = -1;
        public void ComputeMatrices() {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            matrices = GetMatrices();
            watch.Stop();
            ComputeMatricesDuration = watch.ElapsedMilliseconds;

            ArrangeClones(true);
        }

        public void ComputeAll() {
            ComputeOperators();
            ComputeMatrices();
        }

        public (Matrix4x4 matrix, float sqrDistance, int index)[] GetSortedByDistanceMatrices(Vector3 position) {

            var ms = matrices.Select((m, index) => {
                var d = GeomUtils.ExtractPosition(transform.localToWorldMatrix * m) - position;
                return (m, d.sqrMagnitude, index);
            }).ToArray();

            System.Array.Sort(ms, (A, B) => (int)(B.sqrMagnitude - A.sqrMagnitude));

            return ms;
        }

        public bool showDebug = false;

#if UNITY_EDITOR
        [CustomEditor(typeof(SuperCloner))]
        class SuperClonerEditor : Editor {
            SuperCloner cloner => target as SuperCloner;
            public override void OnInspectorGUI()  {
                Inspector.Draw(cloner, serializedObject);
            }
        }
 #endif
    }
}