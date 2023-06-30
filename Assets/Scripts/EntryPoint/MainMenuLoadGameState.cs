using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MainMenuLoadGameState : StateMachineState {

    public MainMenuLoadGameState(StateMachine stateMachine) : base(stateMachine) { }

    public List<LoadGameButton> buttons = new();
    public Dictionary<string, Sprite> screenshotSprites = new();

    public override IEnumerator<StateChange> Enter {
        get {
            var view = stateMachine.TryFind<EntryPointState>().view;

            foreach (var go in view.hiddenInLoadGame)
                go.SetActive(false);
            view.loadGameRoot.SetActive(true);

            view.loadGameButtonPrefab.gameObject.SetActive(false);

            var first = true;
            foreach (var savedGame in PersistentData.Read().savedGames) {
                var button = Object.Instantiate(view.loadGameButtonPrefab, view.loadGameButtonPrefab.transform.parent);
                button.gameObject.SetActive(true);
                buttons.Add(button);
                button.guid = savedGame.guid;
                button.saveName.text = savedGame.name;
                button.textUnderlay.ForceUpdate();
                button.button.onClick.AddListener(() => Select(savedGame));
                var screenshotTexture = savedGame.Screenshot;
                if (screenshotTexture) {
                    var screenshotSprite = Sprite.Create(screenshotTexture, new Rect(Vector2.zero, new Vector2(screenshotTexture.width, screenshotTexture.height)), Vector2.one / 2);
                    screenshotSprites.Add(savedGame.guid, screenshotSprite);
                    button.horizontalFitter.Sprite = screenshotSprite;
                }
                else
                    button.horizontalFitter.Sprite = view.missingScreenshotSprite;

                if (first) {
                    first = false;
                    Select(savedGame);
                }
            }

            while (true) {
                var shouldStop = InputState.TryConsumeKeyDown(KeyCode.Escape);
                if (shouldStop)
                    break;
                yield return StateChange.none;
            }

            yield return StateChange.Pop();
        }
    }

    public void Select(SavedGame savedGame) {
        
        var view = stateMachine.TryFind<EntryPointState>().view;
        
        view.savedGameScreenshotImage.sprite = screenshotSprites.TryGetValue(savedGame.guid, out var sprite) ? sprite : null;
        view.savedGameNameText.text = savedGame.name;
        view.savedGameDateTimeText.text = savedGame.dateTime.ToString(CultureInfo.InvariantCulture);
        view.savedGameInfoLeftText.text = string.Format(view.savedGameInfoLeftFormat,
            Gettext._p("LoadGame", "MISSION"),
            Gettext._p("LoadGame", "BRIEF"),
            Strings.GetDescription(savedGame.missionName));
        view.savedGameInfoRightText.text = string.Format(view.savedGameInfoRightFormat,
            Strings.GetName(savedGame.missionName));
    }

    public override void Exit() {
        
        var view = stateMachine.TryFind<EntryPointState>().view;
        
        foreach (var go in view.hiddenInLoadGame)
            go.SetActive(true);
        view.loadGameRoot.SetActive(false);

        foreach (var button in buttons)
            Object.Destroy(button.gameObject);
        buttons.Clear();

        foreach (var sprite in screenshotSprites.Values)
            Object.Destroy(sprite);
        screenshotSprites.Clear();
    }
}