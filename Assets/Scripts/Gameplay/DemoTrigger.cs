using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTrigger : MonoBehaviour {
    
    void OnTriggerSequence (Sequencer.SequenceTrigger trigger) {
        Debug.Log("Hey! Salut toi!\nÇa ne te dérange pas si j'interromps le jeu ?");
        Debug.Break();
    }
}
