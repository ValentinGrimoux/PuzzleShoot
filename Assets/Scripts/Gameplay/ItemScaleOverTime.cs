using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScaleOverTime : MonoBehaviour {

    [Range(0f, 20f)]
    public float power = 10f;

    Item item;
    Vector3 initialScale;
    
    void Start() {
        item = GetComponent<Item>();   
        initialScale = transform.localScale; 
    }

    void FixedUpdate() {
        // https://www.desmos.com/calculator/z103ozusfm
        transform.localScale = initialScale * (1f - Mathf.Pow(item.timeRatio, power));
    }
}
