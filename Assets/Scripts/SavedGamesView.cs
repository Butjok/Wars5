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

public class PersistentData {

    public static PersistentData Read() {
        return PlayerPrefs.GetString(nameof(PersistentData))?.FromJson<PersistentData>() ?? new PersistentData();
    }
    public void Save() {
        PlayerPrefs.SetString(nameof(PersistentData), this.ToJson());
    }

    public bool firstTimeLaunch = true;
    public Campaign campaign = new();
    public List<SavedGame> savedGames = new() {
        new SavedGame {
            name = "Hello",
            screenshotPath = "/Users/butjok/vfedotov.com/playdead/11.PNG"
        },
        new SavedGame {
            name = "World",
            screenshotPath = "/Users/butjok/vfedotov.com/playdead/20.PNG"
        },
    };
    public GameSettings gameSettings = new();
    public List<string> log = new();
}

public class SavedGame {

    public string name;
    public DateTime dateTime;
    public string missionName;
    public string state;
    public string screenshotPath;

    public static string ScreenshotDirectoryPath => Application.persistentDataPath;
    public const string screenshotExtension = ".png";
    public void GenerateScreenshotPath() {
        screenshotPath = Path.ChangeExtension(Path.Combine(ScreenshotDirectoryPath, Guid.NewGuid().ToString()), screenshotExtension);
    }

    public Texture2D LoadScreenshot() {
        if (!File.Exists(screenshotPath))
            return null;
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(screenshotPath), true);
        return texture;
    }
    public void SaveScreenshot(Texture2D texture) {
        var data = texture.EncodeToPNG();
        File.WriteAllBytes(screenshotPath, data);
    }
}