using System;
using UnityEngine;

public class MusicTest : MonoBehaviour {
	public bool playing;
	public AudioClip theme;
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Return)) {
			playing = !playing;
			if (playing)
				Music.Play(theme);
			else
				Music.Stop();
		}
	}
}