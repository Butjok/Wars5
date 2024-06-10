using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class EntryPointView : MonoBehaviour {

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
    [TextArea(10, 10)]
    public string savedGameInfoLeftFormat = @"{0}
{1}
{2}
{3}

{4}";
    [TextArea(10, 10)]
    public string savedGameInfoRightFormat = @"{0}
{1}
{2}";

    public TextFrame3d textFrame3d;


    public TextMeshPro loadGameText;
    public List<GitInfoEntry> gitInfoEntries = new();

    private void OnEnable() {
        var game = Game.Instance;
        if (game.stateMachine.Count == 0)
            game.stateMachine.Push(new EntryPointState(game.stateMachine, showSplash, showWelcome));

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