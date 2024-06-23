using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerThemeAudio : MonoBehaviour {
    private static PlayerThemeAudio instance;
    public static PlayerThemeAudio Instance {
        get {
            if (!instance) {
                var go = new GameObject(nameof(PlayerThemeAudio));
                DontDestroyOnLoad(go);
                instance = go.AddComponent<PlayerThemeAudio>();
            }
            return instance;
        }
    }
    public Dictionary<PersonName, AudioSource> audioSources = new();
    public Dictionary<AudioSource, IEnumerator> coroutines = new();
    public static void Play(PersonName personName) {
        if (!Instance.audioSources.TryGetValue(personName, out var audioSource)) {
            audioSource = Instance.gameObject.AddComponent<AudioSource>();
            audioSource.clip = personName switch {
                PersonName.Natalie => "grenzerkompanie".LoadAs<AudioClip>(),
                PersonName.Vladan => "the-dead-awaken".LoadAs<AudioClip>(),
                _ => throw new ArgumentOutOfRangeException()
            };
            audioSource.loop = true;
            audioSource.spatialize = false;
            audioSource.volume = 1;
            audioSource.outputAudioMixerGroup = "Game".LoadAs<AudioMixer>()?.FindMatchingGroups("Music")[0];
            Instance.audioSources[personName] = audioSource;
        }

        if (Instance.coroutines.TryGetValue(audioSource, out var previousCoroutine)) {
            Instance.StopCoroutine(previousCoroutine);
            Instance.coroutines.Remove(audioSource);
        }

        var coroutine = FadeCoroutine(audioSource, true);
        Instance.StartCoroutine(coroutine);
        Instance.coroutines[audioSource] = coroutine;

        audioSource.time = 0;
        audioSource.Play();
    }
    public static void Stop() {
        foreach (var audioSource in Instance.audioSources.Values) {
            if (Instance.coroutines.TryGetValue(audioSource, out var previousCoroutine)) {
                Instance.StopCoroutine(previousCoroutine);
                Instance.coroutines.Remove(audioSource);
            }
            if (audioSource.isPlaying) {
                var coroutine = FadeCoroutine(audioSource, false);
                Instance.StartCoroutine(coroutine);
                Instance.coroutines[audioSource] = coroutine;
            }
        }
    }

    public static IEnumerator FadeCoroutine(AudioSource audioSource, bool on, float speed = 2) {
        var targetVolume = on ? 1f : 0f;
        var duration = Mathf.Abs(audioSource.volume - targetVolume) / speed;
        while (duration > 0) {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, Time.deltaTime * speed);
            duration -= Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume;
        if (!on)
            audioSource.Stop();
        Instance.coroutines.Remove(audioSource);
    }

    public AudioMixer audioMixer;
    public void Start() {
        audioMixer = "Game".LoadAs<AudioMixer>();
    }

    public static bool ToneDownMusic {
        set => Instance.audioMixer.SetFloat("MusicLowpass", value ? 200 : 22000);
    }
}