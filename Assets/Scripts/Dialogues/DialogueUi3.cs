using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * TODO:
 * 1) Add a way to change the speaker's name position.
 * 2) Add speech shaking e.g. because of explosion impact.
 */

public class DialogueUi3 : MonoBehaviour {

    public static Color GetTextColor(PersonName personName) {
        return personName switch {
            PersonName.Natalie => Color.white,
            PersonName.Vladan => Color.yellow,
            PersonName.JamesWillis => Color.blue,
            PersonName.LjubisaDragovic => Color.red,
            _ => Color.white
        };
    }
    
    public TMP_Text speakerName;
    public TMP_Text text;
    public PortraitStack[] portraitStacks = { };
    private AudioSource voiceOverSource, sfxSource;
    public Button buttonPrefab;
    public float buttonSpacing = 25;
    public Image spaceKey;
    
    public bool ShowSpaceKey {
        set => spaceKey.enabled = value;
    }
    public bool Visible {
        set => gameObject.SetActive(value);
    }
    public AudioSource VoiceOverSource {
        get {
            if (!voiceOverSource) {
                voiceOverSource = gameObject.AddComponent<AudioSource>();
                voiceOverSource.spatialBlend = 0;
            }
            return voiceOverSource;
        }
    }
    public AudioSource SfxSource {
        get {
            if (!sfxSource) {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.spatialBlend = 0;
            }
            return sfxSource;
        }
    }
    public string Text {
        get => text.enabled ? text.text : null;
        set {
            if (value != null) {
                text.enabled = true;
                text.text = value;
            }
            else
                text.enabled = false;
        }
    }
    public PersonName? Speaker {
        set {
            if (value is { } personName) {
                speakerName.enabled = true;
                speakerName.text = Gettext._(Persons.GetFirstName(personName));
                text.enabled = true;
                text.color = DialogueUi3.GetTextColor(personName);
            }
            else {
                speakerName.enabled = false;
                text.enabled = false;
            }
        }
    }

    public void Reset() {
        ShowSpaceKey = false;
        Text = null;
        Speaker = null;
    }
}