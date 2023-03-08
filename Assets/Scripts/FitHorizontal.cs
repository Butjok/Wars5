using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class FitHorizontal : MonoBehaviour {

    public float desiredWidth = 250;
    public Image image;

    public Sprite[] testSprites = { };

    private void OnEnable() {
        image = GetComponent<Image>();
        Assert.IsTrue(image);
    }

    private int index = 0;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.P) && testSprites.Length > 0)
            Sprite = testSprites[index++ % testSprites.Length];
    }

    public Sprite Sprite {
        set {

            image.sprite = value;

            float width = value.texture.width;
            float height = value.texture.height;
            var aspect = height / width;

            var rectTransform = GetComponent<RectTransform>();
            Assert.IsTrue(rectTransform);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, desiredWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, desiredWidth * aspect);
        }
    }
}