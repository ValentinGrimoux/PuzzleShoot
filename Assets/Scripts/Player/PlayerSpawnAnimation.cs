using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnAnimation : MonoBehaviour {

    void Update() {

        Vector3 target = Stage.instance.Bottom + Vector3.up * 2f;
        transform.position += (target - transform.position) * 2f * Time.deltaTime;

        Item.timeScale += (1f - Item.timeScale) * 2f * Time.deltaTime;
    }
}
