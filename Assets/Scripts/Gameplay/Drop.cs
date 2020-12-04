using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour {

    public GameObject prefab;

    bool applicationIsQuitting = false;
    void OnApplicationQuit() {
        applicationIsQuitting = true;
    }

    void OnDestroy() {
        
        if (prefab != null && applicationIsQuitting == false) {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
