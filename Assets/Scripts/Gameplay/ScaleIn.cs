using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleIn : MonoBehaviour {

    public float scaleSpeed = 8f;

    public float Scale {
        get => transform.localScale.x;
        set => transform.localScale = Vector3.one * value;
    }

    void Start() {
        Scale = 0f;
    }

    void Update() {
        float newScale = Scale + (1f - Scale) * scaleSpeed * Time.deltaTime * Item.timeScale;
        Scale = Mathf.Clamp01(newScale);

        if (Scale > 0.9999f) {
            Scale = 1f;
            Destroy(this); // auto-destruction du composant (pas du gameobject !)
        }
    }
}
