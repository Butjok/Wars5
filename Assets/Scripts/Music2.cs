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
        source.volume = .5f;
        source.loop = true;
    }

    public const string natalieTheme = "grenzerkompanie_i";
    public const string nataliePowerTheme = "chicherina-trimmed";

    public const string vladanTheme = "normal uzicko";
    public const string vladanPowerTheme = "hardbass";

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F3))
            Play(natalieTheme);
        else if (Input.GetKeyDown(KeyCode.F2)) {
            PostProcessing.SuperPowerMode = false;
            if (source.clip.name is vladanTheme or vladanPowerTheme)
                Play(natalieTheme);
            else
                Play(vladanTheme);
        }
        else if (Input.GetKeyDown(KeyCode.F7)) {
            switch (source.clip.name) {
                case vladanTheme:
                    PostProcessing.SuperPowerMode = true;
                    Play(vladanPowerTheme);
                    break;
                case vladanPowerTheme:
                    PostProcessing.SuperPowerMode = false;
                    Play(vladanTheme);
                    break;
                case natalieTheme:
                    PostProcessing.SuperPowerMode = true;
                    Play(nataliePowerTheme);
                    break;
                case nataliePowerTheme:
                    PostProcessing.SuperPowerMode = false;
                    Play(natalieTheme);
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

public static class Sounds {

    private static AudioSource source;
    public static void PlayOneShot(AudioClip clip) {
        if (!source) {
            var gameObject = new GameObject(nameof(Sounds));
            source = gameObject.AddComponent<AudioSource>();
            source.spatialize = false;
        }

        source.PlayOneShot(clip);
    }

    public static readonly AudioClip lightTankMovement = Resources.Load<AudioClip>("song025");
    public static readonly AudioClip mediumTankMovement = Resources.Load<AudioClip>("song027");
    public static readonly AudioClip armorHit = Resources.Load<AudioClip>("song002");
    public static readonly AudioClip explosion = Resources.Load<AudioClip>("song016");
    public static readonly AudioClip rifleShot = Resources.Load<AudioClip>("song019");
    public static readonly AudioClip rocketLauncherShot = Resources.Load<AudioClip>("song021");
    public static readonly AudioClip reconMovement = Resources.Load<AudioClip>("song029");
    public static readonly AudioClip bulletRicochet = Resources.Load<AudioClip>("song049");
    public static readonly AudioClip cannonShot = Resources.Load<AudioClip>("song023");
}