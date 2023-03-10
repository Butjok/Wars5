using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ButtonContrastText : MonoBehaviour {

    public Button button;
    public TMP_Text text;
    
    private void Reset() {
        button = GetComponent<Button>();
        Assert.IsTrue(button);
        text = button.GetComponentInChildren<TMP_Text>();
        Assert.IsTrue(text);
    }

    private void LateUpdate() {
        Color32 buttonColor = button.targetGraphic.canvasRenderer.GetColor();
        text.color = buttonColor.YIQContrastColor();
    }
}