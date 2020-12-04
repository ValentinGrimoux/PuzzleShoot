using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour {

    Item item;

    void Start() {
        item = GetComponentInParent<Item>();
    }

    void Update() {

        if (item != null) {
            transform.GetChild(0).localScale = new Vector3(item.HpRatio, 1f, 1f);
        }
    }
}
