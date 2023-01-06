using System.Collections.Generic;
using Butjok.CommandLine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoadGameMenu : MonoBehaviour {

    public Main main;
    public GameObject root;
    public Image screenshotImage;
    public Button entryButtonPrefab;
    public RectTransform entriesContainer;
    public string entryButtonTextFormat = "{2} - {1}";
    public TMP_Text descriptionText;
    [TextArea(5, 10)]
    public string descriptionTextFormat = "FILENAME: {0}\nNAME: {1}\nDATE: {2}\nSCENE: {3}";
    public SaveEntry entry;
    public Button deleteButton;

    public Dictionary<string, Sprite> screenshotCache = new();
    public List<Button> entryButtons = new();

    public void Show(Main main, IEnumerable<SaveEntry> saveEntries) {

        this.main = main;
        root.SetActive(true);

        foreach (var entry in saveEntries) {
            var button = Instantiate(entryButtonPrefab, entriesContainer);
            entryButtons.Add(button);
            button.gameObject.SetActive(true);
            button.onClick.AddListener(() => Select(entry));
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text = Format(entryButtonTextFormat, entry);
        }
    }

    public void Select(SaveEntry entry) {
        this.entry = entry;
        if (!screenshotCache.TryGetValue(entry.fileName, out var sprite)) {
            sprite = SaveEntry.LoadScreenshot(entry.fileName);
            screenshotCache.Add(entry.fileName, sprite);
        }
        screenshotImage.sprite = sprite;
        descriptionText.text = Format(descriptionTextFormat, entry);
    }

    private string Format(string format, SaveEntry entry) {
        return string.Format(format, entry.fileName, entry.name, entry.dateTime, entry.sceneName);
    }

    public void Hide() {
        root.SetActive(false);
        foreach (var button in entryButtons)
            Destroy(button.gameObject);
        entryButtons.Clear();
    }

    public void Close() {
        main.commands.Enqueue(LoadGameState.close);
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();

        deleteButton.interactable = entry != null;
    }

    public void DeleteEntry() {
        if (entry == null)
            return;

    }
}