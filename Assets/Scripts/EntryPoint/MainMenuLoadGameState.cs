using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MainMenuLoadGameState : StateMachineState {

    public enum Command {LoadGame, Cancel}
    
    public MainMenuLoadGameState(StateMachine stateMachine) : base(stateMachine) { }

    public List<LoadGameButton> buttons = new();
    public Dictionary<string, Sprite> screenshotSprites = new();

    public override IEnumerator<StateChange> Enter {
        get {
            var gameSession = stateMachine.Find<GameSessionState>();
            var game = gameSession.game;
            var persistentData = gameSession.persistentData;
            var view = stateMachine.Find<MainMenuState2>().view;
            var panel = view.loadGamePanel;

            var saves = new List<SavedMission>();
            foreach (var mission in persistentData.campaign.Missions)
                saves.AddRange(mission.saves);
            saves.Sort((a, b) => a.dateTimeUtc.CompareTo(b.dateTimeUtc));

            panel.Show(saves, savedMission => game.EnqueueCommand(Command.LoadGame, savedMission), () => game.EnqueueCommand(Command.Cancel));

            while (true) {
                yield return StateChange.none;
                
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
                    game.EnqueueCommand(Command.Cancel);
                
                while (game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.LoadGame, SavedMission savedMission):
                            yield return StateChange.PopThenPush(3, new LoadingState(stateMachine, savedMission));
                            break;
                        case (Command.Cancel, _):
                            yield return StateChange.Pop();
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
            }
        }
    }

    //public void Select(SavedGame savedGame) {

    /*var view = stateMachine.TryFind<EntryPointState>().view;
    
    view.savedGameScreenshotImage.sprite = screenshotSprites.TryGetValue(savedGame.id, out var sprite) ? sprite : null;
    view.savedGameNameText.text = savedGame.name;
    view.savedGameDateTimeText.text = savedGame.dateTime.ToString(CultureInfo.InvariantCulture);
    view.savedGameInfoLeftText.text = string.Format(view.savedGameInfoLeftFormat,
        Gettext._p("LoadGame", "MISSION"),
        Gettext._p("LoadGame", "BRIEF"),
        Strings.GetDescription(savedGame.missionName));
    view.savedGameInfoRightText.text = string.Format(view.savedGameInfoRightFormat,
        Strings.GetName(savedGame.missionName));*/
    //}

    public override void Exit() {
        
        var view = stateMachine.Find<MainMenuState2>().view;
        var panel = view.loadGamePanel;

        panel.Hide();
    }
}