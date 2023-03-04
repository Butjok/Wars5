using System;
using TMPro;
using UnityEngine;
using static Gettext;

[ExecuteInEditMode]
public class SetTextToLocalizedGameObjectName : MonoBehaviour {

    public string context = "";
    
    private void Awake() {
        var text = GetComponentInChildren<TMP_Text>();
        if (text)
            text.text = string.IsNullOrWhiteSpace(context) ? _(name) : _p(context,name);
    }

    private void Update() {
        if (!Application.isPlaying) {
            var text = GetComponentInChildren<TMP_Text>();
            if (text)
                text.text = name;
        }
    }
}