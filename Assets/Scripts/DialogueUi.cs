using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class DialogueUi : MonoBehaviour {

    private static DialogueUi instance;
    public static DialogueUi Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<DialogueUi>(true);
                Assert.IsTrue(instance);
            }
            return instance;
        }
    }

    public TMP_Text text;
    public TMP_Text portrait;
    public bool typeText = true;
    public IEnumerator textTypingAnimation;
    public AudioSource voiceOverSource;

    public IEnumerator TextTypingAnimation(char[] text) {
        if (!this.text)
            yield break;
        this.text.text = string.Empty;
        if (text == null)
            yield break;
        for (var i = 0; i < text.Length; i++) {
            this.text.SetText(text, 0, i);
            yield return null;
        }
    }

    [Serializable]
    public class Speech {
        public DialogueSpeaker speaker;
        public Line[] lines = Array.Empty<Line>();
    }

    [Serializable]
    public class Line {
        [TextArea] public string text = string.Empty;
        public AudioClip voiceOver;
        public Action action;
        public DialogueSpeaker.Mood? moodChange;
    }

    public Dictionary<string, char[]> stringCache = new();
    public Dictionary<DialogueSpeaker, DialogueSpeaker.Mood> moods = new();

    public bool Visible {
        get => gameObject.activeSelf;
        set {
            if (!Visible)
                moods.Clear();
            gameObject.SetActive(value);
        }
    }

    public void Refresh(DialogueSpeaker speaker, Line line) {

        if (line.moodChange is { } newMood)
            moods[speaker] = newMood;
        
        if (portrait) {
            //portrait.sprite = portrait && speaker ? speaker.portrait : null;
            //portrait.enabled = portrait.sprite;
            
            var position = portrait.rectTransform.anchoredPosition;
            position.x = Mathf.Abs(position.x) * (speaker ? speaker.side : -1);
            portrait.rectTransform.anchoredPosition = position;

            if (!moods.TryGetValue(speaker, out var mood))
                mood = moods[speaker] = DialogueSpeaker.Mood.Normal;

            portrait.text = mood.ToString();
        }

        if (text) {
            if (textTypingAnimation != null)
                StopCoroutine(textTypingAnimation);
            if (!stringCache.TryGetValue(line.text, out var charArray))
                charArray = stringCache[line.text] = line.text.ToCharArray();
            textTypingAnimation = TextTypingAnimation(charArray);
            StartCoroutine(textTypingAnimation);
        }

        if (VoiceOverSource.isPlaying)
            VoiceOverSource.Stop();
        if (line.voiceOver)
            VoiceOverSource.PlayOneShot(line.voiceOver);

        line.action?.Invoke();
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
}