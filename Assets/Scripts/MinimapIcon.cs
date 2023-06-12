using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MinimapIcon : MonoBehaviour {

    public MinimapUi ui;
    public Transform target;
    public Image image;
    public Rect worldBounds;

    private void Awake() {
        image = GetComponent<Image>();
        Assert.IsTrue(image);
    }
    private void LateUpdate() {
        if (!target)
            Destroy(gameObject);
        if (target.gameObject.activeSelf) {
            image.enabled = true;
            image.rectTransform.anchoredPosition = (target.position.ToVector2() - worldBounds.center) * ui.unitSize;
            image.rectTransform.rotation = Quaternion.identity;
        }
        else
            image.enabled = false;
    }
}