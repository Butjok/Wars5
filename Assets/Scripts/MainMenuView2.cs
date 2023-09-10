using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuView2 : MonoBehaviour {
    
    public MainMenuButton campaignButton, loadGameButton, settingsButton, aboutButton, quitButton;
    public GameSettingsMenu gameSettingsMenu;
    public Image holdImage;
    public GameObject aboutRoot;
    public ScrollRect aboutScrollRect;
    public VideoPlayer splashScreenVideoPlayer;
    public VideoClip splashScreenVideoClip;
    public GameObject pressAnyKey;
    public Camera mainCamera;
    
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

    public void OnGUI() {
        if (gitInfo != null) {
            var lastEntry = gitInfo.OrderByDescending(entry => entry.DateTime).FirstOrDefault();
            if (lastEntry != null) {
                GUI.skin = DefaultGuiSkin.TryGet;
                var text = $"git {lastEntry.commit} @ {lastEntry.DateTime}";
                var content = new GUIContent(text);
                var size = GUI.skin.label.CalcSize(content);
                var position = new Vector2(Screen.width-size.x, Screen.height - size.y);
                GUI.Label(new Rect(position, size), content);
            }
        }
    }
}