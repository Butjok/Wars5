using System.Collections.Generic;
using System.Linq;
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
        source.loop = false;
        source.volume = .0f;
    }

    private IEnumerator<AudioClip> queue;
    public IEnumerator<AudioClip> Queue {
        get => queue;
        set {
            queue = value;
            queue.MoveNext();
            if (source.isPlaying) {
                if (source.clip != queue.Current)
                    source.Stop();
                else
                    queue.MoveNext();
            }
        }
    }

    public void Update() {
        
        if (queue == null || source.isPlaying)
            return;
        
        var clip = queue.Current;
        if (clip) {
            source.clip = clip;
            source.Play();
            queue.MoveNext();
        }
    }
}