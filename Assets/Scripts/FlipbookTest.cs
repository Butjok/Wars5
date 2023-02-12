using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlipbookTest : MonoBehaviour {
    public Image image;
    public string spriteName = "";
    public Sprite[] sprites = { };
    private void Start() {
        sprites = Resources.LoadAll<Sprite>(spriteName);
        if (sprites.Length > 0)
            StartCoroutine(Loop());
    }
    public IEnumerator Loop() {
        var index = 0;
        while (true) {
            image.sprite = sprites[index++ % sprites.Length];
            yield return new WaitForSeconds(.1f);
        }
    }
}