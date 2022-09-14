using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    private static MusicPlayer instance;
    public static MusicPlayer Instance {
        get {
            if (!instance) {
                var gameObject = new GameObject(nameof(MusicPlayer));
                DontDestroyOnLoad(gameObject);
                instance = gameObject.AddComponent<MusicPlayer>();
            }
            return instance;
        }
    }

    public AudioSource source;
    public void Awake() {
        source = gameObject.AddComponent<AudioSource>();
    }

    public IEnumerator<AudioClip> queue;

    public void Update() {
        
        if (queue == null || source.isPlaying)
            return;
        
        queue.MoveNext();
        var clip = queue.Current;
        if (clip)
            source.PlayOneShot(clip);
    }
}

