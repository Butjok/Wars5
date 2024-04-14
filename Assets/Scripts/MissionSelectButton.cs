using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MissionSelectButton : MonoBehaviour {

    public Image thumbnail;
    public Image checkmark;
    public Button button;
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text pleaseComplete;
    public Color textUnlockedColor = Color.white;
    public Color textLockedColor = new Color(0, 0, 0, .66f);

    public void Reset() {
        button = GetComponent<Button>();
    }

    public bool startUnlocked = true;

    [Command]
    public bool Unlocked {
        set {
            title.color =  value ? textUnlockedColor : textLockedColor;
            var titleContrast = title.GetComponent<TextContrast>();
            if (titleContrast)
                titleContrast.enabled = value;
            description.enabled = value;
            var descriptionContrast = description.GetComponent<TextContrast>();
            if (descriptionContrast)
                descriptionContrast.enabled = value;
            pleaseComplete.enabled = !value;
            button.interactable = value;
            thumbnail.enabled = value;
        }
    }
    [Command]
    public bool ShowCheckmark {
        set => checkmark.enabled = value;
    }

    public void OnEnable() {
        Unlocked = startUnlocked;
    }
}