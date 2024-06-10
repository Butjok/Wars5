using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavedGameListView : MonoBehaviour {

    public int perPage = 10;

    public Button previousPageButton;
    public Button nextPageButton;
    public TMP_Text pageText;
    public string pageTextFormat = "{0} / {1}";
    public RectTransform buttonContainer;
    public Button buttonPrefab;
    public bool hideButtons = false;
    public TMP_InputField pageInputField;

    public SavedGameView savedGameView;

    private void OnEnable() {
        if (buttonPrefab)
            buttonPrefab.gameObject.SetActive(false);
        TryShowPage(0);
    }

    public List<Action> cleanUpPageActions = new();
    private int? oldPage;

    [UsedImplicitly]
    public void SubmitPageNumber() {
        if ((!int.TryParse(pageInputField.text, out var page) || !TryShowPage(page - 1)) && oldPage is { } value)
            pageInputField.text = (value + 1).ToString();
    }

    private void OnDisable() {
        foreach (var action in cleanUpPageActions)
            action();
        cleanUpPageActions.Clear();
    }

    [Command]
    public bool TryShowPage(int page) {

        /*Assert.IsTrue(buttonContainer);
        Assert.IsTrue(buttonPrefab);

        var savedGames = new List<SavedGame>();
        for (var i = 0; i < 95; i++)
            savedGames.Add(new SavedGame { name = Funny.Message() });
        // var savedGames = PersistentData.Loaded.savedGames;
        var pagesCount = savedGames.PagesCount(perPage);
        if (page < 0 || page >= pagesCount)
            return false;

        foreach (var action in cleanUpPageActions)
            action();
        cleanUpPageActions.Clear();

        var first = true;
        var slice = savedGames.GetPage(perPage, page);
        foreach (var savedGame in slice) {

            if (first) {
                first = false;
                if (savedGameView)
                    savedGameView.SavedGame = savedGame;
            }

            var button = Instantiate(buttonPrefab, buttonContainer);
            button.gameObject.SetActive(true);
            cleanUpPageActions.Add(() => Destroy(button.gameObject));

            if (savedGameView)
                button.onClick.AddListener(() => savedGameView.SavedGame = savedGame);

            var text = button.GetComponentInChildren<TMP_Text>();
            var image = button.GetComponentInChildren<Image>();

            if (text)
                text.text = savedGame.name;

            Texture2D texture;
            if (File.Exists(savedGame.ScreenshotPath)) {
                texture = new Texture2D(2, 2);
                texture.LoadImage(File.ReadAllBytes(savedGame.ScreenshotPath), true);
                cleanUpPageActions.Add(() => Destroy(texture));
            }
            else
                texture = "screenshot".LoadAs<Texture2D>();

            if (image && texture) {
                var rectTransform = button.GetComponent<RectTransform>();
                var buttonWidth = rectTransform.sizeDelta.x;
                var buttonHeight = rectTransform.sizeDelta.y;
                var aspectRatio = buttonHeight / buttonWidth;
                var textureWidth = texture.width;
                var textureHeight = texture.height;
                var usedTextureHeight = textureWidth * aspectRatio;
                var sprite = Sprite.Create(texture, new Rect(0, (textureHeight - usedTextureHeight) / 2, textureWidth, usedTextureHeight), Vector2.zero);
                image.sprite = sprite;

                cleanUpPageActions.Add(() => Destroy(sprite));
            }
        }

        if (pageText)
            pageText.text = string.Format(pageTextFormat, page + 1, pagesCount);
        if (pageInputField)
            pageInputField.text = (page + 1).ToString();

        if (previousPageButton) {
            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(() => TryShowPage(page - 1));
            var active = page > 0;
            previousPageButton.interactable = active;
            if (hideButtons)
                previousPageButton.gameObject.SetActive(active);
        }

        if (nextPageButton) {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(() => TryShowPage(page + 1));
            var active = page < pagesCount - 1;
            nextPageButton.interactable = active;
            if (hideButtons)
                nextPageButton.gameObject.SetActive(active);
        }

        oldPage = page;*/

        return true;
    }
}