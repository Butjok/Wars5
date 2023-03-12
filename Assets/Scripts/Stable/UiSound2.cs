using UnityEngine;
using Object = UnityEngine.Object;

public enum UiClipName {
    NotAllowed
}

public static class UiSound2 {

    public static AudioSource audioSource;

    public static void PlayOneShot(UiClipName clipName) {
        
        if (!audioSource) {
            var go = new GameObject(nameof(UiSound));
            Object.DontDestroyOnLoad(go);
            audioSource = go.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.spatialize = false;
            audioSource.playOnAwake = false;
            audioSource.volume = 1;
        }
        
        audioSource.PlayOneShot(clipName switch {
            UiClipName.NotAllowed => null,
            _ => null
        });
    }
}