using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuView2 : MonoBehaviour {

    public GameSettingsMenu gameSettingsMenu;
    public Image holdImage;
    public RectTransform aboutRoot;
    public ScrollRect aboutScrollRect;
    public VideoPlayer splashScreenVideoPlayer;
    public VideoClip splashScreenVideoClip;
    public GameObject pressAnyKey;
    public Camera mainCamera;
    public LoadGamePanel loadGamePanel;

    public List<GitInfoEntry> gitInfo;
    public MainMenuQuitDialog quitDialog;

    public Action<MainMenuSelectionState2.Command> select;

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
                select?.Invoke(button.command);
        }
        else if (oldButton) {
            oldButton.HighlightIntensity = 0;
            oldButton = null;
        }
    }

    public void OnGUI() {
        /*if (gitInfo != null) {
            var lastEntry = gitInfo.OrderByDescending(entry => entry.DateTime).FirstOrDefault();
            if (lastEntry != null) {
                GUI.skin = DefaultGuiSkin.TryGet;
                var text = $"git {lastEntry.commit} @ {lastEntry.DateTime}";
                var content = new GUIContent(text);
                var size = GUI.skin.label.CalcSize(content);
                var position = new Vector2(Screen.width - size.x, Screen.height - size.y);
                GUI.Label(new Rect(position, size), content);
            }
        }*/
    }

    public Dictionary<RectTransform, Coroutine> coroutines = new();
    public void TranslatePanel(RectTransform panel, Vector2 targetLeftRight, bool disableOnFinish = false) {
        if (coroutines.TryGetValue(panel, out var coroutine))
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(PanelTranslationAnimation(panel, targetLeftRight, disableOnFinish));
        coroutines[panel] = coroutine;
    }
    public void TranslateShowPanel(RectTransform panel) {
        panel.SetLeft(0).SetRight(-1920);
        panel.gameObject.SetActive(true);
        TranslatePanel(panel, new Vector2(0, 0));
    }
    public void TranslateHidePanel(RectTransform panel) {
        TranslatePanel(panel, new Vector2(0, -1920), true);
    }

    [Command] public Easing.Name easing = Easing.Name.InOutQuad;
    [Command] public float duration = .33f;

    public IEnumerator PanelTranslationAnimation(RectTransform panel, Vector2 targetLeftRight, bool disableOnFinish = false) {
        var startLeftRight = new Vector2(panel.GetLeft(), panel.GetRight());
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.Dynamic(easing, t);
            panel.SetLeft(Mathf.Lerp(startLeftRight[0], targetLeftRight[0], t));
            panel.SetRight(Mathf.Lerp(startLeftRight[1], targetLeftRight[1], t));
            yield return null;
        }

        panel.SetLeft(targetLeftRight[0]);
        panel.SetRight(targetLeftRight[1]);
        if (disableOnFinish)
            panel.gameObject.SetActive(false);
        yield break;
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