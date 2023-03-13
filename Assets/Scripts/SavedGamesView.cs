using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SavedGamesView : MonoBehaviour {

    public Button buttonPrefab;
    public RectTransform buttonsContainer;
    public List<Button> buttons = new();
    public Dictionary<string, Sprite> thumbnails = new();
    public Image thumbnail;

    private void Awake() {
        Assert.IsTrue(buttonPrefab);
        Assert.IsTrue(buttonsContainer);
        Assert.IsTrue(thumbnail);
    }

    private void Start() {
        Show(PersistentData.Get.savedGames);
    }

    public void Show(IEnumerable<SavedGame> savedGames) {

        buttonPrefab.gameObject.SetActive(false);
        thumbnail.enabled = false;

        gameObject.SetActive(true);

        foreach (var savedGame in savedGames) {
            if (thumbnails.ContainsKey(savedGame.ScreenshotPath))
                continue;
            var texture = savedGame.Screenshot;
                var sprite =texture? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero):null;
                thumbnails.Add(savedGame.ScreenshotPath, sprite);
        }

        buttonPrefab.gameObject.SetActive(true);
        foreach (var savedGame in savedGames) {
            var button = Instantiate(buttonPrefab, buttonsContainer);
            buttons.Add(button);
            button.onClick.AddListener(() => Select(savedGame));
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = savedGame.name;
            var image = button.GetComponentInChildren<Image>();
            image.sprite = thumbnails[savedGame.ScreenshotPath];
        }
        buttonPrefab.gameObject.SetActive(false);
    }

    public void Hide() {

        foreach (var button in buttons)
            Destroy(button.gameObject);
        buttons.Clear();

        thumbnail.enabled = false;

        gameObject.SetActive(false);
    }

    public void Select(SavedGame savedGame) {
        var found = thumbnails.TryGetValue(savedGame.ScreenshotPath, out var sprite);
        Assert.IsTrue(found);
        thumbnail.enabled = true;
        thumbnail.sprite = sprite;
        thumbnail.preserveAspect = true;
    }
}

public class SavedGamesSelectionState : IDisposableState {

    public SavedGamesView view;
    public SavedGamesSelectionState(SavedGamesView view) {
        this.view = view;
    }

    public IEnumerator<StateChange> Run {
        get {
            var persistentData = PersistentData.Get;
            var savedGames = persistentData.savedGames;
            var screenShots = savedGames.ToDictionary(savedGame => savedGame.ScreenshotPath, savedGame => savedGame.Screenshot);
            
            
            
            yield break;
        }
    }

    public void Dispose() { }
}