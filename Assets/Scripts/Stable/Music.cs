using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;

public class Music : MonoBehaviour {

    public static Music instance;
    public static Music Instance {
        get {
            if (!instance) {
                var go = new GameObject(nameof(Music));
                DontDestroyOnLoad(go);
                instance = go.AddComponent<Music>();
            }
            return instance;
        }
    }

    public static AudioSource Play(IEnumerable<AudioClip> clips, bool loop = true) {

        var source = Instance.gameObject.AddComponent<AudioSource>();
        source.spatialize = false;
        source.volume = 1;
        source.loop = false;

        Instance.StartCoroutine(PlaySequence(source, loop ? clips.InfiniteSequence() : clips));

        return source;
    }

    public static AudioSource Play(AudioClip clip, bool loop = true) {
        return Play(new[] { clip }, loop);
    }

    private static IEnumerator PlaySequence(AudioSource source, IEnumerable<AudioClip> clips) {
        foreach (var clip in clips) {
            yield return null;
            if (!source)
                yield break;
            source.clip = clip;
            source.Play();
            while (source && source.isPlaying)
                yield return null;
        }
    }

    [Command]
    public static float fadeSpeed = 1;

    private static void Fade(AudioSource source, float targetVolume, bool destroy = true) {
        Instance.StartCoroutine(FadeAnimation(source, targetVolume, fadeSpeed, destroy));
    }
    public static void Mute(AudioSource source) {
        Fade(source, 0, false);
    }
    public static void Unmute(AudioSource source) {
        Fade(source, 1, false);
    }
    public static void Kill(AudioSource source) {
        Fade(source, 0, true);
    }

    private static IEnumerator FadeAnimation(AudioSource source, float targetVolume, float speed, bool destroy) {
        while (!Mathf.Approximately(source.volume, targetVolume)) {
            var volumeChangeThisFrame = speed * Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(source.volume, targetVolume, volumeChangeThisFrame / Mathf.Abs(source.volume - targetVolume));
            yield return null;
        }
        if (Mathf.Approximately(0, targetVolume) && destroy)
            Destroy(source);
    }
}