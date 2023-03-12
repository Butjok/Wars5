using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    private static MusicPlayer instance;
    public static MusicPlayer TryGet() {
        if (instance)
            return instance;
        var gameObject = new GameObject(nameof(MusicPlayer));
        DontDestroyOnLoad(gameObject);
        instance = gameObject.AddComponent<MusicPlayer>();
        return instance;
    }
    public static bool TryGet(out MusicPlayer musicPlayer) {
        musicPlayer = TryGet();
        return musicPlayer;
    }

    public AudioSource source;
    public void Awake() {
        source = gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.volume = 1.0f;
        source.Stop();
    }

    public void StopPlaying() {
        source.Stop();
    }

    public void StartPlaying(IEnumerable<AudioClip> audioClips) {
        StopAllCoroutines();
        StartCoroutine(Animation(audioClips));
    }
    private IEnumerator Animation(IEnumerable<AudioClip> audioClips) {
        source.Stop();
        foreach (var audioClip in audioClips) {
            source.clip = audioClip;
            source.Play();
            while (source.isPlaying)
                yield return null;
        }
        source.Stop();
    }
}