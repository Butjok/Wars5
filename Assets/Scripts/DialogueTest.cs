using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTest : MonoBehaviour {

	public TMP_Text text;
	public Image portrait;
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
	}

	public Vector2Int index;
	public Speech[] speeches = Array.Empty<Speech>();
	public Dictionary<string, char[]> stringCache = new();

	public void Update() {
		if (Input.GetKeyDown(KeyCode.PageDown)) {
			var speech = speeches[index[0]];
			if (index[1] < speech.lines.Length - 1)
				index[1]++;
			else {
				index[0]++;
				index[1] = 0;
			}
			if (IsValidIndex(index))
				Refresh();
			else
				enabled = false;
		}
	}

	public void OnEnable() {
		gameObject.SetActive(true);
		Refresh();
	}
	public void OnDisable() {
		gameObject.SetActive(false);
	}

	public bool IsValidIndex(Vector2Int index) {
		return
			index[0] >= 0 && index[0] < speeches.Length &&
			index[1] >= 0 && index[1] < speeches[index[0]].lines.Length;
	}

	public void Refresh() {

		enabled = IsValidIndex(index);
		if (!enabled)
			return;

		var speech = speeches[index[0]];
		var line = speech.lines[index[1]];

		if (portrait) {
			portrait.sprite = portrait && speech.speaker ? speech.speaker.portrait : null;
			portrait.enabled = portrait.sprite;
			var position = portrait.rectTransform.anchoredPosition;
			position.x = Mathf.Abs(position.x) * (speech.speaker ? speech.speaker.side : -1);
			portrait.rectTransform.anchoredPosition = position;
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
	}

	public void OnValidate() {
		index[0] = Mathf.Clamp(index[0], 0, speeches.Length - 1);
		index[1] = Mathf.Clamp(index[1], 0, speeches[index[0]].lines.Length - 1);
		//Refresh();
	}

	public AudioSource VoiceOverSource {
		get {
			if (!voiceOverSource) {
				var go = new GameObject();
				DontDestroyOnLoad(go);
				voiceOverSource = go.AddComponent<AudioSource>();
				voiceOverSource.spatialBlend = 0;
			}
			return voiceOverSource;
		}
	}
}