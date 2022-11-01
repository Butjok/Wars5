using System.Collections.Generic;
using UnityEngine;

public class LoadGameMenu : MonoBehaviour {

    public Game game;
    public GameObject root;

    public void Show(Game game, List<SaveData2> saves) {
        this.game = game;
        root.SetActive(true);

        saves.Sort((a, b) => a.dateTime.CompareTo(b.dateTime));
    }
    public void Hide() {
        root.SetActive(false);
    }

    public void Close() {
        LoadGameState.shouldBreak = true;
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
}