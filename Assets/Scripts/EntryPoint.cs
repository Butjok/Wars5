using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EntryPoint : MonoBehaviour {

    public Main main;
    public LevelLogic levelLogic;
    public string startState = "SplashState";
    
    private void Awake() {

        Assert.AreEqual(1, FindObjectsOfType<EntryPoint>(true).Length);

        Assert.IsTrue(videoPlayer);
        Assert.IsTrue(videoPlayer.targetCamera);
        Assert.IsTrue(bulkaGamesIntro);
        Assert.IsTrue(mainCamera);
        Assert.IsTrue(pressAnyKeyText);

        Assert.IsTrue(startVirtualCamera);
        Assert.IsTrue(logoVirtualCamera);
        Assert.IsTrue(mainMenuVirtualCamera);

        gameObject.name = nameof(EntryPoint);
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {

        mainCamera.enabled = false;
        pressAnyKeyText.enabled = false;

        startVirtualCamera.enabled = true;
        logoVirtualCamera.enabled = false;
        mainMenuVirtualCamera.enabled = false;

        StartCoroutine(SplashState());
    }

    public VideoPlayer videoPlayer;
    public VideoClip bulkaGamesIntro;
    public Camera mainCamera;

    public bool skipSplash = true;
    public bool splashCompleted;

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera logoVirtualCamera;
    public CinemachineVirtualCamera mainMenuVirtualCamera;

    public IEnumerator SplashState() {

        if (!skipSplash) {

            videoPlayer.enabled = true;
            videoPlayer.targetCamera.enabled = true;

            videoPlayer.clip = bulkaGamesIntro;
            videoPlayer.Play();
            splashCompleted = false;
            videoPlayer.loopPointReached += _ => splashCompleted = true;

            while (!splashCompleted) {

                // debug
                // break;
                
                yield return null;
                if (Input.anyKeyDown)
                    break;
            }

            videoPlayer.enabled = false;
            videoPlayer.targetCamera.enabled = false;
        }

        yield return WelcomeState();
    }

    public float fadeDuration = 2;
    public Ease fadeEasing = Ease.Unset;
    public TMP_Text pressAnyKeyText;
    public float delay = 1;
    public IEnumerator WelcomeState() {

        PostProcessing.ColorFilter = Color.black;
        mainCamera.enabled = true;

        PostProcessing.Fade(Color.white, fadeDuration, fadeEasing);

        startVirtualCamera.enabled = false;
        logoVirtualCamera.enabled = true;

        var pressAnyKeySequence = DOTween.Sequence();
        pressAnyKeySequence
            .SetDelay(delay)
            .AppendCallback(() => pressAnyKeyText.enabled = true);

        while (true) {
            
            // debug
            // break;
            
            yield return null;
            if (Input.anyKeyDown)
                break;
        }
        
        pressAnyKeySequence.Kill();
        pressAnyKeyText.enabled = false;
        
        yield return MainMenuState();
    }

    public IEnumerator MainMenuState() {
        Debug.Log("MAIN MENU");

        logoVirtualCamera.enabled = false;
        mainMenuVirtualCamera.enabled = true;

        while (true) {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Return))
                SceneManager.LoadScene("SampleScene");
        }
    }
}