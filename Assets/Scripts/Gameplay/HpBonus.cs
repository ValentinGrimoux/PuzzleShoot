using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBonus : MonoBehaviour {

    void OnTriggerEnter(Collider other) {

        if (other.gameObject.CompareTag("Player")) {
            Player.player.GiveOneHp();
            Destroy(gameObject);
        }
    }
}
