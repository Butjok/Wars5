using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class UiRelativeScale : MonoBehaviour {

    public RectTransform target;
    public float scale = 1;
    public bool clamp = true;
    public Vector2 range = new(0,1000);

    public RectTransform rectTransform;
    private void Reset() {
        rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);
    }

    public void LateUpdate() {
        if (target) {
            var size = target.rect.size.x * scale;
            if (clamp)
                size = Mathf.Clamp(size, range[0], range[1]);
            rectTransform.sizeDelta = new Vector2(size,size);
        }
    }
}