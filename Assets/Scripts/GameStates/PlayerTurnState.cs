using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerTurnState : StateMachineState {

    public PlayerTurnState(StateMachine stateMachine) : base(stateMachine) { }

    public Player player;
    public AudioSource musicSource;

    [Command] public static bool triggerVladansSpeech = false;

    public override IEnumerator<StateChange> Enter {
        get {
            var level = stateMachine.Find<LevelSessionState>().level;
            player = level.CurrentPlayer;
            player.view.Show(player.uiPosition, player.Credits, player.AbilityMeter, Rules.MaxAbilityMeter(player), player.UiColor, player.coName);
            Debug.Log($"Start of turn #{level.turn}: {player}");

            var musicTracks = Persons.GetMusicThemes(player.coName).ToList();
            if (musicTracks.Count > 0)
                musicSource = Music.Play(musicTracks);

            var turnButton = level.view.turnButton;
            if (turnButton) {
                turnButton.Color = player.UiColor;
                var animation = turnButton.PlayAnimation(level.turn / level.players.Count);
                while (!animation() && !Input.anyKeyDown)
                    yield return StateChange.none;
            }

            switch (level.Day()) {
                case 0:
                    switch (level.CurrentPlayer.ColorName) {
                        case ColorName.Red:
                            yield return StateChange.Push(new TutorialStartDialogue(stateMachine));
                            break;
                        case ColorName.Blue:
                            yield return StateChange.Push(new TutorialVladansTurnDialogue(stateMachine));
                            break;
                    }
                    break;
            }

            yield return StateChange.Push(new SelectionState(stateMachine));
        }
    }
    public override void Exit() {

        player.view.Hide();

        if (musicSource)
            Music.Kill(musicSource);

        Debug.Log($"End of turn");
    }
}

public class DayChangeState : StateMachineState {
    public DayChangeState(StateMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator<StateChange> Enter {
        get {
            var levelView = stateMachine.Find<LevelSessionState>().level.view;
            if (levelView.sun) {
                var animation = levelView.sun.PlayDayChange();
                //while (!animation())
                //       yield return StateChange.none;
            }
            yield break;
        }
    }
}