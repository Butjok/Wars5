using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class UiSound {

	public static AudioSource source;
	public static UiSounds sounds;
	
	static UiSound() {

		var go = new GameObject(nameof(UiSound));
		Object.DontDestroyOnLoad(go);
		source = go.AddComponent<AudioSource>();
		source.loop = false;
		source.spatialize = false;
		source.playOnAwake = false;
		source.volume = 1;

		sounds = Resources.Load<UiSounds>(nameof(UiSounds));
		Assert.IsTrue(sounds);
	}

	public static void Play(AudioClip clip) {
		if (clip)
			source.PlayOneShot(clip);
	}
	public static void NotAllowed() {
		Play(sounds.notAllowed);
	}
}