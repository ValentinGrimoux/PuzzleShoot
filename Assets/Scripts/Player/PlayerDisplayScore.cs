using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDisplayScore : MonoBehaviour {

    public TextMeshProUGUI text;

    void Start() {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        text?.SetText("Life: {0}/{1}", Player.player.hp, Player.player.hpMax);
    }
}
