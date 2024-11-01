using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextContrast : MonoBehaviour {

    public Graphic graphic;
    public TMP_Text text;
    public float alpha = 1;

    private void OnEnable() {
        alpha = text.color.a;
    }
    private void LateUpdate() {
        if (!graphic || !text) {
            enabled = false;
            return;
        }
        Color32 color = graphic.canvasRenderer.GetColor();
        Color contrastColor = color.YiqContrastColor();
        contrastColor.a = alpha;
        text.color = contrastColor;
    }
}