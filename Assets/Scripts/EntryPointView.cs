using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Butjok.CommandLine;
using Cinemachine;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;
using static Gettext;

public class EntryPointView : MonoBehaviour {

    [JsonObject(MemberSerialization.OptIn)]
    public struct GitInfoEntry {

        [JsonProperty]
        public string commit;
        [JsonProperty]
        public string author;
        [JsonProperty]
        public string date;
        [JsonProperty]
        public string message;

        public DateTime DateTime => DateTime.Parse(date);
    }

    private void Awake() {

        Assert.AreEqual(1, FindObjectsOfType<EntryPointView>(true).Length);

        Assert.IsTrue(videoPlayer);
        Assert.IsTrue(videoPlayer.targetCamera);
        Assert.IsTrue(bulkaGamesIntro);
        Assert.IsTrue(mainCamera);
        Assert.IsTrue(pressAnyKeyText);

        Assert.IsTrue(startVirtualCamera);
        Assert.IsTrue(logoVirtualCamera);
        Assert.IsTrue(mainMenuVirtualCamera);
    }

    public bool showSplash = true;
    public bool showWelcome = true;

    public VideoPlayer videoPlayer;
    public VideoClip bulkaGamesIntro;
    public Camera mainCamera;

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera logoVirtualCamera;
    public CinemachineVirtualCamera mainMenuVirtualCamera;

    public float fadeDuration = 2;
    public Ease fadeEasing = Ease.Unset;
    public TMP_Text pressAnyKeyText;
    public float delay = 1;
    public Image holdImage;

    public GameObject[] hiddenInAbout = { };
    public GameObject[] hiddenInWelcomeScreen = { };
    public GameObject[] hiddenInLoadGame = { };

    public Color inactiveColor = Color.grey;

    public GameObject about;
    public ScrollRect aboutScrollRect;
    public GameObject loadGameRoot;
    public LoadGameButton loadGameButtonPrefab;
    public Sprite missingScreenshotSprite;
    public Image savedGameScreenshotImage;
    public TMP_Text savedGameNameText;
    public TMP_Text savedGameDateTimeText;
    public TMP_Text savedGameInfoLeftText;
    public TMP_Text savedGameInfoRightText;
    [TextArea(10,10)]
    public string savedGameInfoLeftFormat = @"{0}
{1}
{2}
{3}

{4}";
    [TextArea(10,10)]
    public string savedGameInfoRightFormat = @"{0}
{1}
{2}";

    public TextFrame3d textFrame3d;


    public TextMeshPro loadGameText;
    public List<GitInfoEntry> gitInfoEntries = new();

    private void OnEnable() {
        if (GameStateMachine.Instance.IsEmpty)
            GameStateMachine.Instance.Push(new EntryPointState(showSplash, showWelcome));

        var gitInfoJsonTextAsset = Resources.Load<TextAsset>("GitInfo");
        if (gitInfoJsonTextAsset) {
            var json = gitInfoJsonTextAsset.text;
            gitInfoEntries = json.FromJson<List<GitInfoEntry>>().OrderByDescending(e => e.DateTime).ToList();
        }
    }

    private void OnGUI() {
        if (Debug.isDebugBuild && gitInfoEntries.Count > 0) {
            var entry = gitInfoEntries[0];
            GUI.skin = DefaultGuiSkin.TryGet;
            var text = $"git: {entry.commit} @ {entry.DateTime}";
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            var screenSize = new Vector2(Screen.width, Screen.height);
            var position = screenSize - size;
            GUI.Label(new Rect(position, size), text);
        }
    }
}

public class EntryPointState : IDisposableState {

    [Command]
    public static string sceneName = "EntryPoint";

    public bool showSplash, showWelcome;
    public EntryPointState(bool showSplash, bool showWelcome) {
        this.showSplash = showSplash;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            if (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.LoadScene(sceneName);
                yield return StateChange.none;
            }

            var view = Object.FindObjectOfType<EntryPointView>();
            Assert.IsTrue(view);

            if (showSplash)
                yield return StateChange.Push(new SplashState(view, showWelcome));
            else
                yield return StateChange.Push(new MainMenuState(view, showWelcome));
        }
    }

    public void Dispose() {
        // none
    }
}

public class SplashState : IDisposableState {

    public EntryPointView view;
    public bool showWelcome;
    public SplashState(EntryPointView view, bool showWelcome) {
        this.view = view;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.videoPlayer.enabled = true;
            view.videoPlayer.targetCamera.enabled = true;

            view.videoPlayer.clip = view.bulkaGamesIntro;
            view.videoPlayer.Play();
            var splashCompleted = false;
            view.videoPlayer.loopPointReached += _ => splashCompleted = true;

            while (!splashCompleted && !Input.anyKeyDown)
                yield return StateChange.none;
            yield return StateChange.none;

            yield return StateChange.ReplaceWith(new MainMenuState(view, showWelcome));
        }
    }

    public void Dispose() {
        view.videoPlayer.enabled = false;
        view.videoPlayer.targetCamera.enabled = false;
    }
}

public class MainMenuState : IDisposableState {

    public EntryPointView view;
    public bool showWelcome;
    public MainMenuState(EntryPointView view, bool showWelcome) {
        this.view = view;
        this.showWelcome = showWelcome;
    }

    public IEnumerator<StateChange> Run {
        get {
            PostProcessing.ColorFilter = Color.black;
            view.mainCamera.enabled = true;

            PostProcessing.Fade(Color.white, view.fadeDuration, view.fadeEasing);

            if (showWelcome)
                yield return StateChange.Push(new MainMenuWelcomeState(view));
            else
                yield return StateChange.Push(new MainMenuSelectionState(view));
        }
    }

    public void Dispose() {
        PostProcessing.ColorFilter = Color.white;
        view.mainCamera.enabled = false;
    }
}

public class MainMenuWelcomeState : IDisposableState {

    public EntryPointView view;
    public MainMenuWelcomeState(EntryPointView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.logoVirtualCamera.enabled = true;

            foreach (var go in view.hiddenInWelcomeScreen)
                go.SetActive(false);

            var startTime = Time.time;

            while (true) {
                if (Input.anyKeyDown) {
                    yield return StateChange.none;
                    break;
                }
                if (Time.time > startTime + view.delay && !view.pressAnyKeyText.enabled)
                    view.pressAnyKeyText.enabled = true;
                yield return StateChange.none;
            }
            view.pressAnyKeyText.enabled = false;

            yield return StateChange.ReplaceWith(new MainMenuSelectionState(view));
        }
    }

    public void Dispose() {
        view.logoVirtualCamera.enabled = false;
        foreach (var go in view.hiddenInWelcomeScreen)
            go.SetActive(true);
    }
}

public class MainMenuSelectionState : IDisposableState {

    public static bool quit, goToCampaign, goToAbout, goToSettings, goToLoadGame;

    [Command]
    public static float fadeDuration = .25f;
    [Command]
    public static Ease fadeEasing = Ease.Unset;
    [Command]
    public static float quitHoldTime = 1;
    [Command]
    public static bool simulateNoSavedGames = false;

    public EntryPointView view;
    public Color defaultColor;
    public MainMenuSelectionState(EntryPointView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.mainMenuVirtualCamera.enabled = true;
            view.textFrame3d.gameObject.SetActive(true);

            defaultColor = view.loadGameText.color;
            if (PersistentData.Loaded.savedGames.Count == 0 || simulateNoSavedGames)
                view.loadGameText.color = view.inactiveColor;

            while (true) {

                if (quit) {
                    quit = false;
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    yield break;
                }

                if (goToCampaign) {
                    goToCampaign = false;
                    PostProcessing.ColorFilter = Color.white;
                    var tween = PostProcessing.Fade(Color.black, fadeDuration, fadeEasing);
                    while (tween.IsActive() && !tween.IsComplete())
                        yield return StateChange.none;
                    yield return StateChange.PopThenPush(3, new CampaignOverviewState2());
                }

                if (goToLoadGame) {
                    goToLoadGame = false;
                    if (view.loadGameText.color != view.inactiveColor) {
                        yield return StateChange.Push(new MainMenuLoadGameState(view));
                        continue;
                    }
                    else 
                        UiSound.Instance.notAllowed.PlayOneShot();
                }

                if (goToSettings) {
                    goToSettings = false;
                }

                if (goToAbout) {
                    goToAbout = false;
                    yield return StateChange.Push(new MainMenuAboutState(view));
                    continue;
                }

                if (InputState.TryConsumeKeyDown(KeyCode.Escape)) {

                    var startTime = Time.time;
                    view.holdImage.enabled = true;

                    while (!InputState.TryConsumeKeyUp(KeyCode.Escape)) {

                        var holdTime = Time.time - startTime;
                        if (holdTime > quitHoldTime) {
                            quit = true;
                            break;
                        }

                        view.holdImage.fillAmount = holdTime / quitHoldTime;
                        yield return StateChange.none;
                    }

                    view.holdImage.enabled = false;
                    continue;
                }

                yield return StateChange.none;
            }
        }
    }

    public void Dispose() {
        view.mainMenuVirtualCamera.enabled = false;
        view.loadGameText.color = defaultColor;
        view.textFrame3d.gameObject.SetActive(false);
    }
}

public class MainMenuAboutState : IDisposableState {

    public EntryPointView view;
    public MainMenuAboutState(EntryPointView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            view.about.SetActive(true);
            foreach (var go in view.hiddenInAbout)
                go.SetActive(false);

            view.aboutScrollRect.verticalNormalizedPosition = 1;

            while (true) {
                var shouldStop = InputState.TryConsumeKeyDown(KeyCode.Escape);
                if (shouldStop)
                    break;
                yield return StateChange.none;
            }

            yield return StateChange.Pop();
        }
    }

    public void Dispose() {
        view.about.SetActive(true);
        view.about.SetActive(false);
        foreach (var go in view.hiddenInAbout)
            go.SetActive(true);
    }
}

public class MainMenuLoadGameState : IDisposableState {

    public EntryPointView view;
    public MainMenuLoadGameState(EntryPointView view) {
        this.view = view;
    }

    public List<LoadGameButton> buttons = new();
    public Dictionary<string, Sprite> screenshotSprites = new();

    public IEnumerator<StateChange> Run {
        get {

            foreach (var go in view.hiddenInLoadGame)
                go.SetActive(false);
            view.loadGameRoot.SetActive(true);

            view.loadGameButtonPrefab.gameObject.SetActive(false);

            var first = true;
            foreach (var savedGame in PersistentData.Loaded.savedGames) {
                var button = Object.Instantiate(view.loadGameButtonPrefab, view.loadGameButtonPrefab.transform.parent);
                button.gameObject.SetActive(true);
                buttons.Add(button);
                button.guid = savedGame.guid;
                button.saveName.text = savedGame.name;
                button.textUnderlay.ForceUpdate();
                button.button.onClick.AddListener(() => Select(savedGame));
                var screenshotTexture = savedGame.Screenshot;
                if (screenshotTexture) {
                    var screenshotSprite = Sprite.Create(screenshotTexture, new Rect(Vector2.zero, new Vector2(screenshotTexture.width, screenshotTexture.height)), Vector2.one / 2);
                    screenshotSprites.Add(savedGame.guid,screenshotSprite);
                    button.horizontalFitter.Sprite = screenshotSprite;
                }
                else
                    button.horizontalFitter.Sprite = view.missingScreenshotSprite;

                if (first) {
                    first = false;
                    Select(savedGame);
                }
            }

            while (true) {
                var shouldStop = InputState.TryConsumeKeyDown(KeyCode.Escape);
                if (shouldStop)
                    break;
                yield return StateChange.none;
            }

            yield return StateChange.Pop();
        }
    }

    public void Select(SavedGame savedGame) {
        view.savedGameScreenshotImage.sprite = screenshotSprites.TryGetValue(savedGame.guid, out var sprite) ? sprite : null;
        view.savedGameNameText.text = savedGame.name;
        view.savedGameDateTimeText.text = savedGame.dateTime.ToString(CultureInfo.InvariantCulture);
        view.savedGameInfoLeftText.text = string.Format(view.savedGameInfoLeftFormat, 
            _p("LoadGame", "MISSION"),
            _p("LoadGame", "BRIEF"),
            Strings.GetDescription(savedGame.missionName));
        view.savedGameInfoRightText.text=string.Format(view.savedGameInfoRightFormat,
            Strings.GetName(savedGame.missionName));
    }

    public void Dispose() {
        foreach (var go in view.hiddenInLoadGame)
            go.SetActive(true);
        view.loadGameRoot.SetActive(false);
        
        foreach (var button in buttons)
            Object.Destroy(button.gameObject);
        buttons.Clear();
        
        foreach (var sprite in screenshotSprites.Values)
            Object.Destroy(sprite);
        screenshotSprites.Clear();
    }
}