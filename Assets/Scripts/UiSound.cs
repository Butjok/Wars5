using UnityEngine;
using UnityEngine.Assertions;

public static class UiSound {

	public static AudioSource source;
	
	static UiSound() {

		var go = new GameObject(nameof(UiSound));
		Object.DontDestroyOnLoad(go);
		source = go.AddComponent<AudioSource>();
		source.loop = false;
		source.spatialize = false;
		source.playOnAwake = false;
		source.volume = 1;
	}

	public static void Play(this AudioClip clip) {
		if (clip)
			source.PlayOneShot(clip);
	}
}