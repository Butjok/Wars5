using System;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;

public class GameMenu : MonoBehaviour {

    public Level level;
    public GameObject root;
    public TMP_Text versionText;
    public string versionFormat = "© Copyright {2} 2022–{3}, v {1}";
    public TMP_Text titleText;
    public string titleFormat = "{0}";

    public void Quit() {
        Application.Quit();
    }
    public void Resume() {
        GameMenuState.shouldResume = true;
    }
    public void OpenLoadGame() {
        GameMenuState.shouldLoadGame = true;
    }
    public void OpenSettings() {
        GameMenuState.shouldOpenSettings = true;
    }

    public void Show(Level level) {
        this.level = level;
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
        return string.Format(input, Application.productName, Application.version, Application.companyName, DateTime.UtcNow.Year);
    }
}

public class Campaign {
    
}
public abstract class Mission {
    public string name;
    public string description;
    public Sprite thumbnail;
    public abstract void Load(Level level);
    public virtual LevelLogic LevelLogic() => new();

}