using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UiCircle : MonoBehaviour {

    public Image image;
    public Vector3? position;
    public float radius;
    public Camera camera;
    public Vector3 offset;

    private void Reset() {
        image = GetComponent<Image>();
        Assert.IsTrue(image);
    }

    private void LateUpdate() {
        if (image && camera) {
            if (position is { } actualPosition && camera.TryCalculateScreenCircle(actualPosition + offset, radius, out var center, out var halfSize)) {
                //Draw.ingame.Cross(actualPosition);
                image.enabled = true;
                image.rectTransform.EncapsulateScreenRect(center, new Vector2(halfSize, halfSize));
                image.materialForRendering.SetFloat("_Size", halfSize * 2);
            }
            else
                image.enabled = false;
        }
    }
}