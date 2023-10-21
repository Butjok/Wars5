using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavedGameView : MonoBehaviour {

    public TMP_Text dateTimeText;
    public TMP_Text nameText;
    public Image screenshotImage;
    public Texture2D screenshotTexture;
    public Sprite screenshotSprite;
    public Button loadButton;

    public List<Action> cleanUpActions = new();

    /*public SavedGame SavedGame {
        set {

            foreach (var action in cleanUpActions)
                action();
            cleanUpActions.Clear();

            screenshotSprite = null;
            screenshotTexture = null;

            if (File.Exists(value.ScreenshotPath)) {
                screenshotTexture = new Texture2D(2, 2);
                screenshotTexture.LoadImage(File.ReadAllBytes(value.ScreenshotPath), true);
                cleanUpActions.Add(() => Destroy(screenshotTexture));
            }
            else
                screenshotTexture = "screenshot".LoadAs<Texture2D>();

            screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), Vector2.zero);
            cleanUpActions.Add(() => Destroy(screenshotSprite));

            if (nameText)
                nameText.text = value.name;
            if (dateTimeText)
                dateTimeText.text = value.dateTime.ToString(CultureInfo.InvariantCulture);
            if (screenshotImage)
                screenshotImage.sprite = screenshotSprite;

            if (loadButton) {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => { });
            }
        }
    }*/
}