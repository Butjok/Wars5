using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;
using static Gettext;
using Checker = System.Func<StateMachineState, object, (bool, StateMachineState)>;

public class TutorialDialogue : DialogueState {

    public enum Part {
        WelcomePleaseSelectInfantry,
        WrongSelectionPleaseSelectInfantry,
        PleaseCaptureBuilding,
        WrongPathSelectionPleaseMoveToBuilding,
        WrongActionSelectionPleaseCaptureBuilding,
        NiceJobStartedCapturingBuilding,
        ActionSelectionExplanation,
        ExplainTurnEnd,
        ExplainApc
    }

    public Part part;
    public TutorialDialogue(StateMachine stateMachine, Part part) : base(stateMachine) {
        this.part = part;
    }

    [Command]
    public static bool ExplainMovement {
        get => PlayerPrefs.GetInt(nameof(ExplainMovement), 1) != 0;
        set => PlayerPrefs.SetInt(nameof(ExplainMovement), value ? 1 : 0);
    }

    public override IEnumerator<StateChange> Enter {
        get {
            if (!Level.EnableTutorial)
                yield break;

            var persistentData = stateMachine.Find<GameSessionState>().persistentData;
            var level = stateMachine.Find<LevelSessionState>().level;
            var cameraRig = level.view.cameraRig;
            var players = level.players;
            var player = level.CurrentPlayer;
            var enemy = level.players.SingleOrDefault(p => Rules.AreEnemies(player, p));
            var hqs = level.buildings.Values.Where(building => building.type == TileType.Hq).ToDictionary(hq => hq.Player);
            var units = level.units.Values.Where(u => u.Player == player).ToList();
            var infantryUnit = units.SingleOrDefault(u => u.type == UnitType.Infantry);

            Assert.IsTrue(hqs.ContainsKey(player));
            Assert.IsNotNull(enemy);
            Assert.IsTrue(hqs.ContainsKey(enemy));
            Assert.IsNotNull(infantryUnit);

            Start();
            yield return AddPerson(PersonName.Natalie, DialogueUi4.Side.Left);
            Speaker = PersonName.Natalie;

            Music2.ToneDownVolume();

            switch (part) {
                case Part.WelcomePleaseSelectInfantry: {
                    yield return SayWait(_("Welcome to the Blue Army Commander!"), voiceOverClipName: "tut0");
                    yield return SayWait(_("My name is Natalie, and I will be your guide through this tutorial."), voiceOverClipName: "tut1");
                    yield return SayWait(_("I will help you get started with the basics of the game."), voiceOverClipName: "tut2");
                    yield return SayWait(_("In turns you will be able to move your units, capture buildings, and attack enemy units."), voiceOverClipName: "tut3");

                    if (ExplainMovement) {
                        ui.wasdImage.enabled = true;
                        yield return SayWait(_("Move the camera around using WASD keys and rotate it using Q and E keys. You can try in now!"), voiceOverClipName: "tut4");
                        ui.wasdImage.enabled = false;

                        ui.movement2.enabled = true;
                        yield return SayWait(_("To change the elevation use [1] and [2] keys.\nZoom in and out using the mouse wheel."), voiceOverClipName: "tut5");
                        yield return SayWait(_("You can also hold the middle mouse button move around that way.\nDouble click with middle mouse button to jump to a location."), voiceOverClipName: "tut6");
                        ui.movement2.enabled = false;
                    }

                    if (hqs[player].position.TryRaycast(out var hit)) {
                        cameraRig.enabledMovements = CameraRig.MovementType.FixedInPosition;

                        var completed = cameraRig.Jump(hit.point);
                        while (!completed())
                            yield return StateChange.none;
                        using (ui.PulsateCircle(hit.point))
                            yield return SayWait(_("This is your base, the HQ building.\nIf it is captured, you lose the game.\nIf you capture the enemy HQ, you win the game."), voiceOverClipName: "tut7");

                        if (hqs[enemy].position.TryRaycast(out hit)) {
                            completed = cameraRig.Jump(hit.point);
                            while (!completed())
                                yield return StateChange.none;
                            using (ui.PulsateCircle(hit.point))
                                yield return SayWait(_("Here is the enemy HQ."), voiceOverClipName: "tut8");
                        }

                        cameraRig.enabledMovements = CameraRig.MovementType.All;
                    }

                    if (infantryUnit.NonNullPosition.TryRaycast(out hit)) {
                        var completed = cameraRig.Jump(hit.point);
                        while (!completed())
                            yield return StateChange.none;
                        using (ui.PulsateCircle(hit.point))
                            yield return SayWait(_("Now please select this infantry unit with a left click."), voiceOverClipName: "tut9");
                    }

                    break;
                }

                case Part.WrongSelectionPleaseSelectInfantry: {
                    if (infantryUnit.NonNullPosition.TryRaycast(out var hit)) {
                        var completed = cameraRig.Jump(hit.point);
                        while (!completed())
                            yield return StateChange.none;
                        using (ui.PulsateCircle(hit.point))
                            yield return SayWait(_("Please select this infantry unit."), voiceOverClipName: "tut10");
                    }

                    break;
                }

                case Part.PleaseCaptureBuilding: {
                    yield return SayWait(_("This is an infantry unit, it can move through rivers and capture building."), voiceOverClipName: "tut11");
                    var unownedCity = level.Buildings.SingleOrDefault(b => b.Player == null && b.type == TileType.Factory);
                    Assert.IsTrue(unownedCity != null);
                    Assert.IsTrue(unownedCity.position.TryRaycast(out var hit));
                    using (ui.PulsateCircle(hit.point))
                        yield return SayWait(_("Now please move it to the building on the left and start capturing it."), voiceOverClipName: "tut12");
                    break;
                }

                case Part.WrongPathSelectionPleaseMoveToBuilding:
                    yield return SayWait(_("Please move the infantry unit to the building on the left and order capturing it."), voiceOverClipName: "tut13");
                    break;

                case Part.WrongActionSelectionPleaseCaptureBuilding:
                    yield return SayWait(_("Please select capture order, use [Tab] to cycle through available actions. Press [Space] to confirm."), voiceOverClipName: "tut14");
                    break;

                case Part.ActionSelectionExplanation:
                    yield return SayWait(_("Here you can see the available actions for the selected unit. You can simply stay on the current tile or start capturing the building."), voiceOverClipName: "tut15");
                    yield return SayWait(_("Please select capture order, use [Tab] to cycle through available actions. Press [Space] to confirm."), voiceOverClipName: "tut16");
                    break;

                case Part.NiceJobStartedCapturingBuilding:
                    yield return SayWait(_("Nice job! You have started capturing the building."), voiceOverClipName: "tut17");
                    yield return SayWait(_("Once the capture is complete, the building will be yours. It takes two turns to capture a building if the unit at full health."), voiceOverClipName: "tut18");
                    yield return SayWait(_("Captured building will provide you with additional income and will allow you to produce new units."), voiceOverClipName: "tut19");
                    break;

                case Part.ExplainTurnEnd:
                    yield return SayWait(_("Once you are done with your turn, press the turn button on top of a screen or push [F2]."), voiceOverClipName: "tut20");
                    break;

                case Part.ExplainApc:
                    yield return SayWait(_("This is an APC. It can carry one infantry unit and it can supply friendly units with ammo and petrol."), voiceOverClipName: "tut21");
                    break;
            }


            yield return RemovePerson(DialogueUi4.Side.Left);
            End();
        }
    }

    public override void Exit() {
        Music2.RestoreVolume();
    }
}

public class TutorialVladansTurnDialogue : DialogueState {
    public TutorialVladansTurnDialogue(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Start();
            yield return AddPerson(PersonName.Vladan, DialogueUi4.Side.Right);
            Speaker = PersonName.Vladan;
            yield return SayWait(_("Finally! My turn!"));
            yield return RemovePerson(DialogueUi4.Side.Right);
            End();
        }
    }
}