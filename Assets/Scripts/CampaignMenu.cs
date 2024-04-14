using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CampaignMenu : MonoBehaviour {

    public RectTransform root;
    public Button continueButton;
    public Button selectMissionButton;
    public Button loadGameButton;
    public RectTransform missionSelectRoot;

    public Action continueCampaign;
    public Action loadGame;
    public Action selectMission;

    public void ContinueCampaign() {
        continueCampaign?.Invoke();
    }
    public void LoadGame() {
        loadGame?.Invoke();
    }
    public void SelectMission() {
        selectMission?.Invoke();
    }

    public bool ShowContinueButton {
        set => continueButton.gameObject.SetActive(value);
    }
    public bool ShowSelectMissionButton {
        set => selectMissionButton.gameObject.SetActive(value);
    }
    public bool ShowLoadGameButton {
        set => loadGameButton.gameObject.SetActive(value);
    }
}

public class MainMenuCampaignState : StateMachineState {

    public enum Command { Cancel, Continue, SelectMission }

    public MainMenuCampaignState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var gameSession = stateMachine.Find<GameSessionState>();
            var persistentData = gameSession.persistentData;

            if (!persistentData.campaign.wasStarted)
                yield return StateChange.PopThenPush(2, new LoadingState(stateMachine, new SavedMission {
                    mission = persistentData.campaign.tutorial,
                    input = LevelEditorFileSystem.TryReadLatest("Tutorial")
                }));

            var mainMenu = stateMachine.Find<MainMenuState2>().view;
            var menu = mainMenu.campaignMenu;
            var saves = new List<SavedMission>();
            foreach (var mission in gameSession.persistentData.campaign.Missions)
                saves.AddRange(mission.saves);
            var latestSave = saves.OrderBy(s => s.dateTimeUtc).LastOrDefault();

            menu.ShowContinueButton = latestSave != null;
            menu.continueCampaign = () => gameSession.game.EnqueueCommand(Command.Continue);

            menu.ShowSelectMissionButton = true;
            menu.selectMission = () => gameSession.game.EnqueueCommand(Command.SelectMission);

            menu.ShowLoadGameButton = saves.Count > 0;
            menu.loadGame = () => gameSession.game.EnqueueCommand(MainMenuSelectionState2.Command.OpenLoadGameMenu);

            mainMenu.TranslateShowPanel(menu.root);

            while (true) {
                yield return StateChange.none;

                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
                    gameSession.game.EnqueueCommand(Command.Cancel);

                while (gameSession.game.TryDequeueCommand(out var command)) {
                    switch (command) {
                        case (Command.Cancel, _):
                            yield return StateChange.Pop();
                            break;
                        case (Command.Continue, _):
                            yield return StateChange.PopThenPush(3, new LoadingState(stateMachine, latestSave));
                            break;
                        case (Command.SelectMission, _): {
                            yield return StateChange.Push(new MissionSelectMainMenuState(stateMachine));
                            break;
                        }
                        case (MainMenuSelectionState2.Command name, _):
                            if (name != MainMenuSelectionState2.Command.GoToCampaignOverview)
                                if (name == MainMenuSelectionState2.Command.OpenLoadGameMenu) {
                                    mainMenu.TranslateHidePanel(menu.root);
                                    yield return StateChange.Push(new MainMenuLoadGameState(stateMachine));
                                    mainMenu.TranslateShowPanel(menu.root);
                                }
                                else {
                                    gameSession.game.EnqueueCommand(name);
                                    yield return StateChange.Pop();
                                }
                            break;
                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }
                }
            }
        }
    }

    public override void Exit() {
        var mainMenu = stateMachine.Find<MainMenuState2>().view;
        mainMenu.TranslateHidePanel(mainMenu.campaignMenu.root);
    }
}

public class MissionSelectMainMenuState : StateMachineState {
    public MissionSelectMainMenuState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var mainMenu = stateMachine.Find<MainMenuState2>().view;
            var menu = mainMenu.campaignMenu;
            mainMenu.TranslateHidePanel(menu.root);
            mainMenu.TranslateShowPanel(menu.missionSelectRoot);

            while (true) {
                yield return StateChange.none;
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right))
                    break;
            }
        }
    }
    public override void Exit() {
        var mainMenu = stateMachine.Find<MainMenuState2>().view;
        var menu = mainMenu.campaignMenu;
        mainMenu.TranslateHidePanel(menu.missionSelectRoot);
        mainMenu.TranslateShowPanel(menu.root);
    }
}