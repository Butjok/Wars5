using System;
using TMPro;
using UnityEngine;

public class GameMenu : MonoBehaviour {

    public Main main;
    public GameObject root;
    public TMP_Text versionText;
    public string versionFormat = "© Copyright {2} 2022–{3}, v {1}";
    public TMP_Text titleText;
    public string titleFormat = "{0}";

    public void Show(Main main) {
        this.main = main;
        root.SetActive(true);
        titleText.text = Format(titleFormat);
        versionText.text = Format(versionFormat);
    }
    public void Hide() {
        root.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            main.commands.Enqueue(GameMenuState.close);
    }
    public void EnqueueCommand(string command) {
        main.commands.Enqueue(command);
    }

    private static string Format(string input) {
        return string.Format(input, Application.productName, Application.version, Application.companyName, DateTime.UtcNow.Year);
    }
}

