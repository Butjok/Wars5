using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuView2 : MonoBehaviour {

    public MainMenuButton campaignButton, loadGameButton, settingsButton, aboutButton, quitButton;
    public GameSettingsMenu gameSettingsMenu;
    public Image holdImage;
    public RectTransform aboutRoot;
    public ScrollRect aboutScrollRect;
    public VideoPlayer splashScreenVideoPlayer;
    public VideoClip splashScreenVideoClip;
    public GameObject pressAnyKey;
    public Camera mainCamera;
    public LoadGamePanel loadGamePanel;

    public IEnumerable<MainMenuButton> Buttons {
        get {
            yield return campaignButton;
            yield return loadGameButton;
            yield return settingsButton;
            yield return aboutButton;
            yield return quitButton;
        }
    }

    public List<GitInfoEntry> gitInfo;
    public MainMenuQuitDialog quitDialog;

    public Action<MainMenuSelectionState2.Command> enqueueCommand;

    public MainMenuButton oldButton;
    private void Update() {
        var ray = mainCamera.FixedScreenPointToRay(Input.mousePosition);
        MainMenuButton button;
        if (Physics.Raycast(ray, out var hit, float.MaxValue) && (button = hit.collider.GetComponent<MainMenuButton>())) {
            if (oldButton && oldButton != button)
                oldButton.HighlightIntensity = 0;
            button.HighlightIntensity = 1;
            //Draw.ingame.Cross(hit.point, Color.yellow);
            oldButton = button;
            if (Input.GetMouseButtonDown(Mouse.left))
                enqueueCommand?.Invoke(button.command);
        }
        else if (oldButton) {
            oldButton.HighlightIntensity = 0;
            oldButton = null;
        }
    }

    public void OnGUI() {
        if (gitInfo != null) {
            var lastEntry = gitInfo.OrderByDescending(entry => entry.DateTime).FirstOrDefault();
            if (lastEntry != null) {
                GUI.skin = DefaultGuiSkin.TryGet;
                var text = $"git {lastEntry.commit} @ {lastEntry.DateTime}";
                var content = new GUIContent(text);
                var size = GUI.skin.label.CalcSize(content);
                var position = new Vector2(Screen.width - size.x, Screen.height - size.y);
                GUI.Label(new Rect(position, size), content);
            }
        }
    }

    public Dictionary< RectTransform, Coroutine> coroutines = new();
    public void TranslatePanel(RectTransform panel, Vector2 targetTopBottom, float duration, bool disableOnFinish = false) {
        if (coroutines.TryGetValue(panel, out var oldCoroutine))
            StopCoroutine(oldCoroutine);
        var coroutine = StartCoroutine(PanelTranslationAnimation(panel, targetTopBottom, duration, disableOnFinish));
        coroutines[panel] = coroutine;
    }
    public void TranslateShowPanel(RectTransform panel, float duration = .33f) {
        panel.SetTop(-1080).SetBottom(1080);
        panel.gameObject.SetActive(true);
        TranslatePanel(panel, new Vector2(0, 0), duration);
    }
    public void TranslateHidePanel(RectTransform panel, float duration = .33f) {
        TranslatePanel(panel, new Vector2(-1080, 1080), duration, true);
    }
    public IEnumerator PanelTranslationAnimation(RectTransform panel, Vector2 targetTopBottom, float duration, bool disableOnFinish = false) {
        var startTopBottom =  new Vector2(panel.GetTop(), panel.GetBottom());
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.OutQuad(t);
            panel.SetTop(Mathf.Lerp(startTopBottom[0], targetTopBottom[0], t));
            panel.SetBottom(Mathf.Lerp(startTopBottom[1], targetTopBottom[1], t));
            yield return null;
        }
        panel.SetTop(targetTopBottom[0]);
        panel.SetBottom(targetTopBottom[1]);
        if (disableOnFinish)
            panel.gameObject.SetActive(false);
    }
}

public static class RectTransformExtensions {
    public static RectTransform SetLeft(this RectTransform rt, float left) {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        return rt;
    }
    public static float GetLeft(this RectTransform rt) {
        return rt.offsetMin.x;
    }

    public static RectTransform SetRight(this RectTransform rt, float right) {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        return rt;
    }
    public static float GetRight(this RectTransform rt) {
        return -rt.offsetMax.x;
    }

    public static RectTransform SetTop(this RectTransform rt, float top) {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        return rt;
    }
    public static float GetTop(this RectTransform rt) {
        return -rt.offsetMax.y;
    }

    public static RectTransform SetBottom(this RectTransform rt, float bottom) {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        return rt;
    }
    public static float GetBottom(this RectTransform rt) {
        return rt.offsetMin.y;
    }
}