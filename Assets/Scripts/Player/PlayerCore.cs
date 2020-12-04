using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerCore : MonoBehaviour {

    public Color invincibleColor = Color.red;

    [Utils.LayerMask]
    public int collideMask;

    void Update() {

        Color color = Player.player.invincible ? invincibleColor : Color.white;
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;    
    }

    bool Collide_NoobVersion(int layer) {
        string name = LayerMask.LayerToName(layer);
        return name == "Enemy" || name == "EnemyBullet";
    }

    bool Collide_DevVersion(int layer) => ((1 << layer) & collideMask) > 0;

    void OnTriggerEnter(Collider other) {

        if (Collide_DevVersion(other.gameObject.layer)) {

            if (Player.player.IsInvincible()) {
                Debug.Log("Haha ! Je suis invincible !!!");
                return;
            }

            Destroy(transform.parent.gameObject);
            
            Player.player.RemoveOneHp();
        }
    }
}
