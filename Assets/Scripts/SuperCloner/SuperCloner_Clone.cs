using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils.SuperCloner {

    public partial class SuperCloner : MonoBehaviour {

        public GameObject[] sources = new GameObject[0];
        public GameObject GetSource(int index) {
            
            if (sources.Length == 0) {
                return null;
            }

            return sources[index % sources.Length];
        }
        public void AddSource(GameObject source) {
            sources = sources.Append(source).ToArray();
        }

        public void UseChildrenAsSources() {
            var list = new List<GameObject>();
            foreach (Transform child in transform) {
                // NOTE: if child could be a clone wrapper, must filter child.
                if (true) {
                    list.Add(child.gameObject);
                    child.gameObject.SetActive(false);
                }
            }
            sources = list.ToArray();
        }

        Clone CreateClone(int index, GameObject source) {
            
            GameObject go;
            if (source != null) {
                go = Instantiate(source, transform.position, transform.rotation);
                go.SetActive(true);
            } else {
                go = new GameObject(string.Format("Empty-{0}(Clone)", index));
            }
            var clone = go.AddComponent<Clone>();
            clone.cloner = this;
            clone.index = index;
            clone.source = source;
            return clone;
        }

        public List<Clone> clones = new List<Clone>();

        public void DestroyClones() {
            foreach(var clone in clones) {
                DestroyImmediate(clone.gameObject);
            }
            clones.Clear();
        }

        public GameObject[] DestroyClonerAndReleaseClones() {
            
            var clonesCopy = clones.Select(clone => clone.gameObject).ToArray();

            foreach(var clone in clones) {
                clone.gameObject.hideFlags = HideFlags.None;
                DestroyImmediate(clone.GetComponent<Clone>());
            }

            clones.Clear();
            DestroyImmediate(gameObject);
            return clonesCopy;
        }

        public int IdealCloneCount => innerEnabled && !dryRun && sources.Length > 0 ? matrices.Length : 0;

        public void ComputeClones() {

            clones = FindObjectsOfType<Clone>().Where(clone => clone.cloner == this).ToList();
            clones.Sort((A, B) => A.index - B.index);
            
            int cloneCount = IdealCloneCount;

            // destroy extra clones
            while(clones.Count > cloneCount) {
                var clone = clones[clones.Count - 1];
                clones.RemoveAt(clones.Count - 1);
                if (clone != null) {
                    DestroyImmediate(clone.gameObject);
                }
            }

            for (int index = 0; index < clones.Count; index++) {
                var clone = clones[index];
                var source = GetSource(index);
                if (clone.source != source) {
                    DestroyImmediate(clone.gameObject);
                    clones[index] = CreateClone(index, source);
                } 
            }

            // complete missing clones
            while(clones.Count < cloneCount) {
                int index = clones.Count;
                var source = GetSource(index);
                clones.Add(CreateClone(index, source));
            }

            ArrangeClones(false);
        }

        public void NeverHideBoth(bool priorityToCloner) {
            if (hideClonerInHierarchy && IdealCloneCount == 0) {
                hideClonerInHierarchy = false;
                Debug.Log("Could not hide cloner (because no clones).");
            }
            if (hideClonerInHierarchy && hideClonesInHierarchy) {
                hideClonerInHierarchy = !priorityToCloner && IdealCloneCount > 0;
                hideClonesInHierarchy = priorityToCloner;
            }
        }

        public void ArrangeClones(bool checkForDirtiness) {

            if (checkForDirtiness && clones.Count != IdealCloneCount) {
                ComputeClones();
                return;
            }

            gameObject.hideFlags = hideClonerInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
            
            int clonerIndex = transform.GetSiblingIndex();

            foreach(var clone in clones) {

                if (checkForDirtiness) {
                    if (clone == null || clone.source != GetSource(clone.index)) {
                        ComputeClones();
                        return;
                    }
                }

                var m = transform.localToWorldMatrix * matrices[clone.index];
                clone.gameObject.hideFlags = hideClonesInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;
                clone.transform.SetParent(transform.parent, false);
                clone.transform.SetSiblingIndex(clonerIndex + 1 + clone.index);
                clone.transform.position = GeomUtils.ExtractPosition(m);
                clone.transform.rotation = m.rotation;
            }
        }

        void OnReset() {
            ComputeAll();
        }

        bool innerEnabled = false;
        void OnEnable() {
            innerEnabled = true;
            if (Application.isPlaying == false) {
                ComputeAll();
            }
        }

        void OnDisable() {
            innerEnabled = false;
        }

        void OnDestroy() {
            DestroyClones();
        }

        void Update() {
            if (Application.isPlaying == false) {
                ComputeMatrices();
            }
        }
    }
}