using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Explosion : MonoBehaviour {

    public const string SOURCE_WARNING = "Pas de sources, pas d'explosion !";

    public enum Mode {
        Random,
        Regular,
    }

    public Item[] sources = new Item[0];
    public int count = 8;

    public Mode mode = Mode.Random;

    public float velocity = 4f;
    public float velocityVariation = 0.5f;

    public float timeMax = .6f;
    public float timeMaxVariation = 0.5f;

    [Space(16)]
    public bool explodeOnDestroy = false;
    public float explodeDelay = 0f;

    [Space(16)]
    public bool hideClonesInHierarchy = true;

    public void Explode() {
        
        if (sources.Length == 0) {
            Debug.LogWarning(SOURCE_WARNING);
            return;
        }

        if (explodeDelay > 0) {
            
            var go = new GameObject();
            go.transform.position = transform.position;
            
            var copy = Handy.DuplicateComponent(this, go);
            copy.explodeDelay = 0f;

            Destroy(go, explodeDelay);
            
            return;
        }

        for (int index = 0; index < count; index++) {
            
            int sourceIndex = mode == Mode.Random ? Random.Range(0, sources.Length) : index % sources.Length;
            Item source = sources[sourceIndex];

            float angle = mode == Mode.Random ? Random.Range(0f, 360f) : (float)index / count * 360f;
            
            Item shard = Instantiate(source, transform.position, Quaternion.Euler(0f, 0f, angle));
            shard.gameObject.hideFlags = hideClonesInHierarchy ? HideFlags.HideInHierarchy : HideFlags.None;

            float vVariation = mode == Mode.Random ? Random.Range(-velocityVariation, velocityVariation) : 0f;
            float shardVelocity = velocity * (1f + vVariation);
            float shardVelocityX = shardVelocity * Mathf.Cos(angle * Mathf.Deg2Rad);
            float shardVelocityY = shardVelocity * Mathf.Sin(angle * Mathf.Deg2Rad);
            shard.velocity = new Vector3(shardVelocityX, shardVelocityY, 0f);

            float tVariation = mode == Mode.Random ? Random.Range(-timeMaxVariation, timeMaxVariation) : 0f;
            shard.timeMax = timeMax * (1f + tVariation);
        }
    }

    bool applicationIsQuitting = false;
    void OnApplicationQuit() {
        applicationIsQuitting = true;
    }
    void OnDestroy() {
        if (explodeOnDestroy && Application.isPlaying && !applicationIsQuitting) {
            Explode();            
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Explosion))]
    class MyEditor : Editor {
        Explosion explosion => target as Explosion;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUILayout.Space(16);
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Explode!")) {
                explosion.Explode();
            }
            GUI.enabled = true;
            if (explosion.sources.Length == 0) {
                EditorGUILayout.HelpBox(SOURCE_WARNING, MessageType.Warning);
            }
            if (GUILayout.Button("Duplicate Component")) {
                Handy.DuplicateComponent(explosion, explosion.gameObject);
            }
        }
    }
#endif
}
