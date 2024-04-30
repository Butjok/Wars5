using System.Collections;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;

public class LocationAndTimeText : MonoBehaviour {

    public TMP_Text text;
    [TextArea(10,20)]
    public string content = "";
    public bool playOnStart = true;
    public float speed = 10;
    
    public void Start() {
        if (playOnStart)
            Play();
    }
    [Command]
    public void Play() {
        StopAllCoroutines();
        StartCoroutine(Animation());
    }
    public IEnumerator Animation() {
        var startTime = Time.time;
        var length = content.Length;
        var duration = length / speed;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            var substringLength = (int)(t * length);
            text.text = content.Substring(0, substringLength);
            yield return null;
        }
        text.text = content;
    }
    
    public bool Visible {
        set => text.enabled = value;
    }
}