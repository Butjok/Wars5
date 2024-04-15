using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Audio;

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
                instance.source.outputAudioMixerGroup =  Resources.Load<AudioMixer>("Game").FindMatchingGroups("Music")[0];
            }

            return instance;
        }
    }

    public AudioSource source;
    public const float volume = .5f;
    public const float tonedDownVolume = .5f;
    public const float lowpassDefault = 22000;
    public const float lowpassTonedDown = 200;
    
    public void Awake() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialize = false;
        source.volume = volume;
        source.loop = true;
    }

    public const string natalieTheme = "grenzerkompanie";
    public const string nataliePowerTheme = "chicherina-trimmed";

    public const string vladanTheme = "the-dead-awaken";
    public const string vladanPowerTheme = "hardbass";

    public void Update() {
        if (Input.GetKeyDown(KeyCode.F3))
            Play(natalieTheme);
        else if (Input.GetKeyDown(KeyCode.F2)) {
            PostProcessing.SuperPowerMode = false;
            if (source && source.clip && source.clip.name is vladanTheme or vladanPowerTheme)
                Play(natalieTheme);
            else
                Play(vladanTheme);
        }
        else if (Input.GetKeyDown(KeyCode.F7) && source && source.clip) {
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
    
    public static void ToneDownVolume() {
        Instance.source.volume = tonedDownVolume;
        Instance.source.outputAudioMixerGroup.audioMixer.SetFloat("MusicLowpass", lowpassTonedDown);
    }
    public static void RestoreVolume() {
        Instance.source.volume = volume;
        Instance.source.outputAudioMixerGroup.audioMixer.SetFloat("MusicLowpass", lowpassDefault);
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

public static class VoiceOver {
    private static AudioSource source;
    public static void PlayOneShot(AudioClip clip) {
        if (!source) {
            var gameObject = new GameObject(nameof(VoiceOver));
            source = gameObject.AddComponent<AudioSource>();
            source.spatialize = false;
        }
        
        source.PlayOneShot(clip);
    }
    public static void Stop() {
        if (source)
            source.Stop();
    }
    public static void PlayOneShot(string clipName) {
        var clip = Resources.Load<AudioClip>(clipName);
        if (clip)
            PlayOneShot(clip);
        else
            Debug.LogError($"Clip not found: {clipName}");
    }
}