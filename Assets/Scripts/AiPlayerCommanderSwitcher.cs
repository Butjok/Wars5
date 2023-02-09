using System;
using UnityEngine;

public class AiPlayerCommanderSwitcher : MonoBehaviour {
    public AiPlayerCommander aiPlayerCommander;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha8)) {
            aiPlayerCommander.enabled = !aiPlayerCommander.enabled;
        }
    }
}