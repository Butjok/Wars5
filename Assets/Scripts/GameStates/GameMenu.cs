using System;
using System.Resources;
using TMPro;
using UnityEngine;

public class GameMenu : MonoBehaviour {

    public Game game;
    public GameObject root;
    public TMP_Text versionText;
    public string versionFormat = "© Copyright {2} {3:yyyy}, v {1}";
    public TMP_Text titleText;
    public string titleFormat = "{0}";
    
    public void Quit() {
        Application.Quit();
    }
    public void Resume() {
        GameMenuState.shouldResume = true;
    }
    public void OpenSettings() {
        GameMenuState.shouldOpenSettings = true;
    }

    public void Show(Game game) {
        this.game = game;
        root.SetActive(true);
        titleText.text = Format(titleFormat);
        versionText.text = Format(versionFormat);
    }
    public void Hide() {
        root.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Resume();
    }

    private static string Format(string input) {
        return string.Format(input, Application.productName, Application.version, Application.companyName, DateTime.Now);
    }
}