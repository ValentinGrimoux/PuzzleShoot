using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sequencer {

    public class Sequence : MonoBehaviour {

        public float sequenceLength = 20f;
        public Vector3 initialPosition;

        void Start() {
            initialPosition = transform.localPosition;
        }

        void OnDrawGizmos() {
            
            Sequencer sequencer = GetComponentInParent<Sequencer>();
            float triggerSize = sequencer?.triggerSize ?? 10f;
            Gizmos.color = sequencer?.gizmosColor ?? Color.red;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.right * sequenceLength / 2f, new Vector3(sequenceLength, triggerSize, 0));

            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.25f);
            int max = Mathf.FloorToInt(triggerSize / 2f);
            for (int i = -max; i <= max; i++) {
                Gizmos.DrawLine(new Vector3(0f, i, 0f), new Vector3(sequenceLength, i, 0f));
            }
        }
    }
}
