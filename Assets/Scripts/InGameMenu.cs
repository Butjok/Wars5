using System;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour {

    public GameObject root;    
    public Action resume;
    public Button resumeButton, exitButton, settingsButton, saveGameButton, loadGameButton;

    public void Show(Action resume) {
        this.resume = resume;
        root.SetActive(true);
    }
    public void Hide() {
        root.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Resume();
    }

    public void Resume() {
        resume?.Invoke();
    }
}