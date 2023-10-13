using System;
using System.Collections;
using Butjok.CommandLine;
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
    public RectTransform videoPanelRoot;
    public RawImage videoPanelImage;
    public float videoPanelStartY;
    public float videoPanelMoveDuration = .5f;
    public float videoPanelOffsetScreenY = 1000;
    public Image underlayImage;
    public Color videoUnderlayDarknessColorTint = Color.white;
    [ColorUsage(false,true)] public Color postProcessingDarknessColorFilter = Color.white;

    public void Awake() {
        videoPanelStartY = videoPanelRoot.anchoredPosition.y;
    }

    [Command]
    public IEnumerator ShowVideoPanel() {
        var position = videoPanelRoot.anchoredPosition;
        position.y = videoPanelOffsetScreenY;
        videoPanelRoot.anchoredPosition = position;
        videoPanelRoot.gameObject.SetActive(true);
        return MoveVideoPanel(videoPanelStartY, videoPanelMoveDuration, false);
    }
    [Command]
    public IEnumerator HideVideoPanel() {
        return MoveVideoPanel(videoPanelOffsetScreenY, videoPanelMoveDuration, true);
    }
    public IEnumerator MoveVideoPanel(float targetY, float duration, bool hide) {
        var startTime = Time.time;
        var startY = videoPanelRoot.anchoredPosition.y;
        var position = videoPanelRoot.anchoredPosition;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.InOutQuad(t);
            position.y = Mathf.Lerp(startY, targetY, t);
            videoPanelRoot.anchoredPosition = position;
            yield return null;
        }
        position.y = targetY;
        videoPanelRoot.anchoredPosition = position;
        if (hide)
            videoPanelRoot.gameObject.SetActive(false);
    }

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

    public void MakeDark() {
        underlayImage.color = videoUnderlayDarknessColorTint;
        PostProcessing.ColorFilter = postProcessingDarknessColorFilter;
        PostProcessing.Blur = true;
    }
    public void MakeLight() {
        underlayImage.color = Color.white;
        PostProcessing.ColorFilter = Color.white;
        PostProcessing.Blur = false;
    }
}