using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LoadGamePanel : MonoBehaviour {

    public RectTransform root;
    public MainMenuView2 mainMenuView;

    //
    public Button buttonPrefab;
    public TMP_Text buttonNameText;
    public TMP_Text buttonInfoText;
    public Image buttonScreenshotImage;
    public RectTransform scrollRectContent;
    public List<Button> buttons = new();

    //
    public TMP_Text nameText;
    public TMP_Text dateText;
    public TMP_Text turnText;
    public TMP_Text difficultyText;
    public TMP_Text descriptionText;
    public Image screenshotImage;

    public Button playButton;
    public Button cancelButton;

    public Dictionary<SavedMission, Sprite> screenshots = new();
    public SavedMission selectedSavedMission;

    public void EnsureScreenshotLoaded(SavedMission savedMission) {
        if (!screenshots.TryGetValue(savedMission, out var sprite)) {
            var texture = savedMission.Screenshot;
            sprite = texture ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
            screenshots.Add(savedMission, sprite);
        }
    }

    public IEnumerator WaitForScreenshotAndSet(Image image, SavedMission savedMission) {
        Sprite sprite;
        while (!screenshots.TryGetValue(savedMission, out sprite))
            yield return null;
        image.sprite = sprite;
    }

    public void Show(IReadOnlyList<SavedMission> saves, Action<SavedMission> playMission, Action cancel) {
        Assert.IsTrue(saves.Count > 0);

        root.gameObject.SetActive(true);
        mainMenuView.TranslateShowPanel(root);
        
        buttonPrefab.gameObject.SetActive(false);

        foreach (var save in saves) {
            buttonNameText.text = save.mission.HumanFriendlyName;
            buttonInfoText.text = save.dateTimeUtc.ToString(CultureInfo.InvariantCulture);
            EnsureScreenshotLoaded(save);
            StartCoroutine(WaitForScreenshotAndSet(buttonScreenshotImage, save));

            var button = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            button.transform.SetParent(scrollRectContent);
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => Show(save));
            buttons.Add(button);
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(() => {
            Assert.IsTrue(selectedSavedMission != null);
            playMission(selectedSavedMission);
        });
        
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => cancel?.Invoke());

        Show(saves[0]);
    }

    public void Show(SavedMission savedMission) {
        selectedSavedMission = savedMission;
        nameText.text = savedMission.mission.HumanFriendlyName;
        dateText.text = savedMission.dateTimeUtc.ToString(CultureInfo.InvariantCulture);
        turnText.text = $"Day {savedMission.day}, turn of {savedMission.turnColor}";
        difficultyText.text = savedMission.difficulty.ToString();
        descriptionText.text = savedMission.mission.Description;
        screenshotImage.sprite = screenshots.TryGetValue(savedMission, out var s) ? s : null;
    }

    public void Hide() {
        //root.SetActive(false);
        mainMenuView.TranslateHidePanel(root);

        foreach (var button in buttons)
            Destroy(button.gameObject);
        buttons.Clear();
    }
}