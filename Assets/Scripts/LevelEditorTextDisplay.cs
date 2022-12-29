using System.Collections.Generic;
using System.Linq;
using TMPro;

public class LevelEditorTextDisplay {

    private readonly Dictionary<string, object> text = new();
    private readonly TMP_Text uiText;

    public LevelEditorTextDisplay(TMP_Text uiText) {
        this.uiText = uiText;
    }
    public void UpdateText() {
        uiText.text = string.Join("\n", text
            .OrderBy(pair => pair.Key)
            .Select(pair => $"{pair.Key}: {pair.Value}"));
    }
    public void Clear() {
        text.Clear();
        UpdateText();
    }
    public void Set(string key, object value) {
        text[key] = value;
        UpdateText();
    }
}