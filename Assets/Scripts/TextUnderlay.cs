using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextUnderlay : MonoBehaviour {

    public TMP_Text text;

    public void ForceUpdate() {
        if (!text)
            return;
        var size = text.GetPreferredValues();
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    private void LateUpdate() {
        ForceUpdate();
    }
}