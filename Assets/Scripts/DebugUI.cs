using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    bool _showUI = false;

    PlayerController pc;

    void Start() {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            _showUI = !_showUI;
        }
    }

    void OnGUI() {
        if (!_showUI) return;

        int row = 0;
        float rowH = Screen.height / 20;

        GUILayout.BeginArea(new Rect(0, row * rowH, Screen.width, 3 * rowH));
            GUILayout.Label("attack: " + (pc.IsAttacking ? "on" : "off"));
        GUILayout.EndArea();
    }
}
