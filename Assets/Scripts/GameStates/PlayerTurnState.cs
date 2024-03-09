using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Windows;
using File = System.IO.File;
using Input = UnityEngine.Input;

public class PlayerTurnState : StateMachineState {

    public PlayerTurnState(StateMachine stateMachine) : base(stateMachine) { }

    public Player player;
    public AudioSource musicSource;

    [Command] public static bool triggerVladansSpeech = false;

    [Command] public static bool EnableDialogues {
        get => PlayerPrefs.GetInt(nameof(EnableDialogues)) != 0;
        set => PlayerPrefs.SetInt(nameof(EnableDialogues), value ? 1 : 0);
    }

    public override IEnumerator<StateChange> Enter {
        get {
            var level = stateMachine.Find<LevelSessionState>().level;
            player = level.CurrentPlayer;
            player.view.Show(player.uiPosition, player.Credits, player.AbilityMeter, Rules.MaxAbilityMeter(player), player.UiColor, player.coName);
            Debug.Log($"Start of turn #{level.turn}: {player}");

            var musicTracks = Persons.GetMusicThemes(player.coName).ToList();
            if (musicTracks.Count > 0)
                musicSource = Music.CreateAudioSource(musicTracks);

            var animations = new List<Func<bool>>();
            
            var turnButton = level.view.turnButton;
            if (turnButton) {
                turnButton.Color = player.UiColor;
                animations.Add(turnButton.PlayAnimation(level.turn / level.players.Count));
            }

            if (level.view.sun && level.Day() > 0 && level.Day() != level.Day(level.turn-1)) 
                animations.Add(level.view.sun.PlayDayChange());

            while (animations.Any(animation => !animation()))
                yield return StateChange.none;

            if (EnableDialogues)
                switch (level.Day()) {
                    case 0:
                        switch (level.CurrentPlayer.ColorName) {
                            case ColorName.Blue:
                                yield return StateChange.Push(new TutorialStartDialogue(stateMachine));
                                break;
                            case ColorName.Red:
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
                while (!animation())
                       yield return StateChange.none;
            }

            yield break;
        }
    }
}