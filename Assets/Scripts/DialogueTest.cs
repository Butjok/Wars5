using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
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

	private Dictionary<Vector2Int, char[]> cache;

	public char[] GetLineText(Vector2Int index) {

		if (cache == null) {
			cache = new Dictionary<Vector2Int, char[]>();
			for (var i = 0; i < speeches.Length; i++)
			for (var j = 0; j < speeches[i].lines.Length; j++)
				cache[new Vector2Int(i, j)] = speeches[i].lines[j].text.ToCharArray();
		}

		return cache.TryGetValue(index, out var charArray) ? charArray : null;
	}

	public Vector2Int index;
	public Speech[] speeches = Array.Empty<Speech>();


	
	public void OnValidate() {

		void hide() {
			if (portrait)
				portrait.enabled = false;
			if (text)
				text.enabled = false;
		}

		if (speeches.Length == 0) {
			hide();
			return;
		}
		
		var speechIndex = Mathf.Clamp(index[0], 0, speeches.Length - 1);
		index[0] = speechIndex;
		
		var speech = speeches[speechIndex];
		if (speech.lines.Length == 0) {
			hide();
			return;
		}
		
		var lineIndex = Mathf.Clamp(index[1], 0, speech.lines.Length - 1);
		index[1] = lineIndex;

		var line = speech.lines[lineIndex];
		var lineText = GetLineText(index);

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
			textTypingAnimation = TextTypingAnimation(lineText);
			StartCoroutine(textTypingAnimation);
		}
		if (VoiceOverSource.isPlaying)
			VoiceOverSource.Stop();
		if (line.voiceOver) 
			VoiceOverSource.PlayOneShot(line.voiceOver);
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