using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class DeactivateTriggerOnDestroy : MonoBehaviour {

    public string targetName;
    public bool makeActive = false;

    bool applicationIsQuitting = false;
    void OnApplicationQuit() {
        applicationIsQuitting = true;
    }

    Sequencer.SequenceTrigger GetTarget() {
        return FindObjectOfType<Sequencer.Sequencer>()?.GetTriggerByName(targetName);
    }

    void Proceed(bool active) {
        
    }

    void OnDestroy() {
        if (applicationIsQuitting == false) {
            GetTarget()?.SetEnabled(makeActive);
        }        
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DeactivateTriggerOnDestroy))]
    class MyEditor : Editor {
        DeactivateTriggerOnDestroy Target => target as DeactivateTriggerOnDestroy;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Make Inactive (Test).")) {
                Target.GetTarget()?.SetEnabled(false);
            }
            if (GUILayout.Button("Make Active (Test).")) {
                Target.GetTarget()?.SetEnabled(true);
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}
