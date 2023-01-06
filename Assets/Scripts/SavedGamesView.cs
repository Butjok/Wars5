using System;
using System.Collections.Generic;
using System.IO;
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
        Show(new PersistentData().savedGames);
    }

    public void Show(IEnumerable<SavedGame> savedGames) {

        buttonPrefab.gameObject.SetActive(false);
        thumbnail.enabled = false;
        
        gameObject.SetActive(true);
        
        foreach (var savedGame in savedGames) {
            if (thumbnails.ContainsKey(savedGame.screenshotPath))
                continue;
            var texture = savedGame.LoadScreenshot();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            thumbnails.Add(savedGame.screenshotPath, sprite);
        }
        
        buttonPrefab.gameObject.SetActive(true);
        foreach (var savedGame in savedGames) {
            var button = Instantiate(buttonPrefab, buttonsContainer);
            buttons.Add(button);
            button.onClick.AddListener(() => Select(savedGame));
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = savedGame.name;
            var image = button.GetComponentInChildren<Image>();
            image.sprite = thumbnails[savedGame.screenshotPath];
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
        var found = thumbnails.TryGetValue(savedGame.screenshotPath, out var sprite);
        Assert.IsTrue(found);
        thumbnail.enabled = true;
        thumbnail.sprite = sprite;
    }
}

