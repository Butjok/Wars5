using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextUnderlay : MonoBehaviour {

    public TMP_Text text;
    public Vector2 margin = new Vector2();

    public void ForceUpdate() {
        if (text) {
            var size = text.GetPreferredValues();
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x + margin.x * 2);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y + margin.y * 2);
        }
    }

    private void Update() {
        ForceUpdate();
    }
}