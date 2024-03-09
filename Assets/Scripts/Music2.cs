using Butjok.CommandLine;
using UnityEngine;

public class Music2 : MonoBehaviour {

    private static Music2 instance;

    public static Music2 Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<Music2>();
                if (instance)
                    DontDestroyOnLoad(instance.gameObject);
                else {
                    var go = new GameObject(nameof(Music2));
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<Music2>();
                }
            }

            return instance;
        }
    }

    public AudioSource source;
    public void Awake() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialize = false;
        source.volume = 1;
        source.loop = true;
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F3))
            Play("grenzerkompanie_i");
        else if (Input.GetKeyDown(KeyCode.F2)) {
            PostProcessing.SuperPowerMode = false;
            if (source.clip.name == "normal uzicko" || source.clip.name == "hardbass")
                Play("grenzerkompanie_i");
            else
                Play("normal uzicko");
        }
        else if (Input.GetKeyDown(KeyCode.F7)) {
            switch (source.clip.name) {
                case "normal uzicko":
                    PostProcessing.SuperPowerMode = true;
                    Play("hardbass");
                    break;
                case "chicherina-trimmed":
                    PostProcessing.SuperPowerMode = false;
                    Play("grenzerkompanie_i");
                    break;
                case "grenzerkompanie_i":
                    PostProcessing.SuperPowerMode = true;
                    Play("chicherina-trimmed");
                    break;
                case "hardbass":
                    PostProcessing.SuperPowerMode = false;
                    Play("normal uzicko");
                    break;
            }
        }
    }

    public static void Play(AudioClip clip) {
        Instance.source.clip = clip;
        instance.source.pitch = clip.name == "sakkijarven" ? 1.1f : 1;
        Instance.source.Play();
    }
    [Command]
    public static void Play(string clipName) {
        var clip = Resources.Load<AudioClip>(clipName);
        if (clip)
            Play(clip);
        else {
            Debug.LogError($"Clip not found: {clipName}");
        }
    }
    public static void Stop() {
        Instance.source.Stop();
    }
}