using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter(Collider other) {

        Item otherItem = other.GetComponent<Item>() ?? other.GetComponentInParent<Item>();
        if (otherItem != null) {
            otherItem.SetDamage(damage);
        }

        Destroy(gameObject);
        GetComponent<Explosion>()?.Explode();
    }
}
