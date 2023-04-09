using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class FitWidth : MonoBehaviour {

    public float desiredWidth = 250;
    public Image image;

    private void OnEnable() {
        image = GetComponent<Image>();
        Assert.IsTrue(image);
        Sprite = image.sprite;
    }

    private void OnValidate() {
        if (image)
        Sprite = image.sprite;
    }

    public Sprite Sprite {
        set {
            image.sprite = value;

            if (!value)
                return;

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