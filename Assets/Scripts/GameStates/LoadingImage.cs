using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadingImage : MonoBehaviour {
    public Image image;
    private void Awake() {
        image = GetComponent<Image>();
        Assert.IsTrue(image);
    }
    private void Start() {
        var size = new Vector2((float)image.sprite.texture.width / image.sprite.texture.height * Screen.width, Screen.height);
        image.rectTransform.sizeDelta = size;
    }
}