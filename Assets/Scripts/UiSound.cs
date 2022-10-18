using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = nameof(UiSound))]
public class UiSound : ScriptableObject {

    public static AudioSource source;
    private static UiSound instance;
    public static UiSound Instance {
        get {
            if (!instance) {
                instance = Resources.Load<UiSound>(nameof(UiSound));
                Assert.IsTrue(instance);
            }
            return instance;
        }
    }

    public AudioClip notAllowed;
    public AudioClip placed;
}

public static class AudioClipExtensions {

    public static void PlayOneShot(this AudioClip clip) {

        if (!clip)
            return;

        Debug.Log(clip.name);

        if (!UiSound.source) {
            var go = new GameObject(nameof(UiSound));
            Object.DontDestroyOnLoad(go);
            UiSound.source = go.AddComponent<AudioSource>();
            UiSound.source.loop = false;
            UiSound.source.spatialize = false;
            UiSound.source.playOnAwake = false;
            UiSound.source.volume = 1;
        }

        UiSound.source.PlayOneShot(clip);
    }

    public static IEnumerator<AudioClip> InfiniteSequence(this IEnumerable<AudioClip> themes, bool shuffle = false) {
        if (themes==null) {
            yield return null;
            yield break;
        }
        var shuffled = shuffle ? themes.OrderBy(_ => Random.value).ToArray() : themes;
        while (true) {
            foreach (var clip in shuffled)
                yield return clip;
        }
    }
}