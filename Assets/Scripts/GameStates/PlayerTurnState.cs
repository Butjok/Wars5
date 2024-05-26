using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

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
            player = Level.CurrentPlayer;
            player.view.Show(player.uiPosition, player.Credits, player.AbilityMeter, Rules.MaxAbilityMeter(player), player.UiColor, player.coName);
            Debug.Log($"Start of turn #{Level.turn}: {player}");

            var musicTracks = Persons.GetMusicThemes(player.coName).ToList();
            if (musicTracks.Count > 0)
                musicSource = Music.CreateAudioSource(musicTracks);

            // Sun and turn button animations
            {
                var animations = new List<Func<bool>>();

                var turnButton = Level.view.turnButton;
                if (turnButton) {
                    turnButton.Color = player.UiColor;
                    animations.Add(turnButton.PlayAnimation(Level.turn / Level.players.Count));
                }

                if (Level.view.sun && Level.Day() > 0 && Level.Day() != Level.Day(Level.turn - 1))
                    animations.Add(Level.view.sun.PlayDayChange());

                while (animations.Any(animation => !animation()))
                    yield return StateChange.none;
            }

            switch (Level.Day()) {
                case 0:
                    switch (Level.CurrentPlayer.ColorName) {
                        case ColorName.Blue:
                            if (Level.EnableTutorial)
                                yield return StateChange.Push(new TutorialDialogue(stateMachine, TutorialDialogue.Part.WelcomePleaseSelectInfantry));
                            break;
                        case ColorName.Red:
                            if (Level.EnableDialogues)
                                yield return StateChange.Push(new TutorialVladansTurnDialogue(stateMachine));
                            break;
                    }

                    break;
            }

            var buildings = Level.Buildings.Where(b => b.Player == Level.CurrentPlayer).ToList();
            var income = buildings.Sum(Rules.Income);
            if (income > 0)
                Level.CurrentPlayer.SetCredits(Level.CurrentPlayer.Credits + income, true);

            foreach (var building in buildings)
                if (Level.TryGetUnit(building.position, out var unit) && Rules.CanRepair(building, unit)) {
                    /*if (Level.CurrentPlayer == Level.localPlayer) {
                        var jumpCompleted = Level.view.cameraRig.Jump(unit.view.body.position);
                        while (!jumpCompleted())
                            yield return StateChange.none;
                    }*/

                    unit.SetHp(unit.Hp + Rules.RepairAmount(building));
                }

            if (Level.CurrentPlayer == Level.localPlayer)
                Level.SetGui("objectives", DrawObjectives);
            else
                Level.RemoveGui("objectives");

            yield return StateChange.Push(new SelectionState(stateMachine));
        }
    }

    [Command] public static Vector2Int objectivesSize = new(300, 500);
    private const float unitCircleRadius = .5f;
    private const float buildingCircleRadius = .75f;

    private void DrawObjectives() {
        return; 
        
        var size = objectivesSize;
        var padding = DefaultGuiSkin.padding;
        GUILayout.BeginArea(new Rect(Screen.width - size.x - padding.x, padding.y, size.x, size.y));

        GUILayout.BeginHorizontal();
        var showObjectives = PlayerPrefs.GetInt("show-objectives", 1) != 0;
        if (GUILayout.Button(showObjectives ? "^" : "v")) {
            showObjectives = !showObjectives;
            PlayerPrefs.SetInt("show-objectives", showObjectives ? 1 : 0);
        }

        GUILayout.Label("Objectives");
        GUILayout.EndHorizontal();

        if (showObjectives) {
            var enemyBuildings = Level.Buildings.Where(b => b.Player != null && Rules.AreEnemies(b.Player, Level.localPlayer)).ToList();
            var enemyHqs = enemyBuildings.Where(b => b.type == TileType.Hq).ToList();
            var enemyFactories = enemyBuildings.Where(b => b.type == TileType.Factory).ToList();
            var enemyUnits = Level.Units.Where(u => Rules.AreEnemies(u.Player, Level.localPlayer)).ToList();

            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);

            GUILayout.BeginHorizontal();
            GUILayout.Label("· Capture enemy HQ ");
            if (enemyHqs.Count > 0 && enemyHqs[0].position.TryRaycast(out var hit) && GUILayout.Button("Show")) {
                Level.view.cameraRig.Jump(hit.point);
                PulseCircle(hit.point, buildingCircleRadius);
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("  <color=#fff5><i>or</i></color>");
            GUILayout.Label("· Destroy all enemy units and don't let them produce more units");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"    Units left:     {enemyUnits.Count}");
            if (enemyUnits.Count > 0 && GUILayout.Button("Show"))
                foreach (var unit in enemyUnits)
                    PulseCircle(unit.view.body.position, unitCircleRadius);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"    Factories left: {enemyFactories.Count}");
            if (enemyFactories.Count > 0 && GUILayout.Button("Show"))
                foreach (var factory in enemyFactories)
                    if (factory.position.TryRaycast(out hit))
                        PulseCircle(hit.point, buildingCircleRadius);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
    private void PulseCircle(Vector3 position, float radius) {
        Level.view.StartCoroutine(PulsingCircle(position, radius));
    }
    private IEnumerator PulsingCircle(Vector3 position, float radius) {
        const float duration = 2;
        const float thickness = 1.5f;

        var startTime = Time.time;
        while (Time.time - startTime < duration) {
            using (Draw.ingame.WithLineWidth(thickness))
                Draw.ingame.CircleXZ(position, Mathf.Lerp(radius - .05f, radius + .05f, Mathf.PingPong(Time.time * 10, 1)), Color.white);
            yield return StateChange.none;
        }
    }

    public override void Exit() {
        player.view.Hide();

        if (musicSource)
            Music.Kill(musicSource);

        Level.RemoveGui("objectives");

        Debug.Log($"End of turn");
    }
}