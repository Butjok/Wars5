using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TextContrast : MonoBehaviour {

    public Graphic graphic;
    public TMP_Text text;

    private void LateUpdate() {
        if (!graphic || !text) {
            enabled = false;
            return;
        }
        Color32 color = graphic.canvasRenderer.GetColor();
        text.color = color.YIQContrastColor();
    }
}