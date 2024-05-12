using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using static Gettext;

public class BorderIncidentScenario : MonoBehaviour {

    public enum Command {
        StartRedRocketeersDialogue
    }

    [Command]
    public void Execute() {
        StartCoroutine(Animation());
    }

    public const float unitSpeed = 2;

    public IEnumerator MoveUnitView(UnitView view, Vector2Int to, Vector2Int? finalDirection = null) {
        var path = new List<Vector2Int> { view.Position };
        path.AddRange(Woo.Traverse2D(view.Position, to));
        return new MoveSequence(view.transform, path, _speed: unitSpeed, _finalDirection: finalDirection).Animation();
    }

    public IEnumerator WaitForState<T>() where T : StateMachineState {
        while (!Game.Instance.stateMachine.IsInState<T>())
            yield return null;
    }
    public IEnumerator Wait(float duration) {
        var startTime = Time.time;
        while (Time.time - startTime < duration)
            yield return null;
    }

    public IEnumerator Animation() {
        var game = Game.Instance;
        var stateMachine = game.stateMachine;
        var level = game.Level;
        var levelView = level.view;
        var cameraRig = levelView.cameraRig;
        var bluePlayer = level.players.Single(p => p.ColorName == ColorName.Blue);
        var redPlayer = level.players.Single(p => p.ColorName == ColorName.Red);
        var redInfantry = level.units[new Vector2Int(30, 8)];

        var recon = level.units[new Vector2Int(21, 9)];
        {
            var movement = MoveUnitView(recon.view, new Vector2Int(27, 7));
            while (movement.MoveNext())
                yield return null;
        }

        var blueInfantry = new Unit {
            Player = bluePlayer,
            type = UnitType.Infantry,
            Position = new Vector2Int(27, 8)
        };
        blueInfantry.Initialize();
        {
            var movement = MoveUnitView(blueInfantry.view, new Vector2Int(29, 8));
            while (movement.MoveNext())
                yield return null;
        }

        stateMachine.Push(new BorderIncidentIntroDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentIntroDialogueState>() != null)
            yield return null;

        {
            var blueInfantryMovement = MoveUnitView(blueInfantry.view, new Vector2Int(27, 8), Vector2Int.right);
            var redInfantryMovement = MoveUnitView(redInfantry.view, new Vector2Int(34, 9), Vector2Int.left);
            var reconMovement = MoveUnitView(recon.view, new Vector2Int(21, 9), Vector2Int.right);
            while (redInfantryMovement != null || blueInfantryMovement != null || reconMovement != null) {
                blueInfantryMovement = blueInfantryMovement != null && blueInfantryMovement.MoveNext() ? blueInfantryMovement : null;
                redInfantryMovement = redInfantryMovement != null && redInfantryMovement.MoveNext() ? redInfantryMovement : null;
                reconMovement = reconMovement != null && reconMovement.MoveNext() ? reconMovement : null;
                yield return null;
            }
        }

        yield return WaitForState<SelectionState>();
        game.EnqueueCommand(SelectionState.Command.EndTurn);
        yield return WaitForState<SelectionState>();

        if (new Vector2Int(43, -6).TryRaycast(out var hit))
            yield return cameraRig.JumpAnimation(hit.point, 2);

        {
            yield return WaitForState<SelectionState>();
            game.EnqueueCommand(Command.StartRedRocketeersDialogue, new BorderIncidentRedRocketeersDialogueState(stateMachine));

            var rocketeer = level.units[new Vector2Int(41, -7)];
            var missileSilo = level.buildings[new Vector2Int(41, -5)];

            game.EnqueueCommand(SelectionState.Command.Select, rocketeer.NonNullPosition);

            yield return WaitForState<PathSelectionState>();
            game.EnqueueCommand(PathSelectionState.Command.ReconstructPath, missileSilo.position);
            game.EnqueueCommand(PathSelectionState.Command.Move);

            yield return WaitForState<ActionSelectionState>();
            game.EnqueueCommand(ActionSelectionState.Command.Execute, new UnitAction(UnitActionType.LaunchMissile, rocketeer, stateMachine.Find<PathSelectionState>().path, targetBuilding: missileSilo));

            yield return WaitForState<MissileTargetSelectionState>();
            stateMachine.Peek<MissileTargetSelectionState>().AimPosition = new Vector2Int(21, 9);

            yield return Wait(1);
            game.EnqueueCommand(MissileTargetSelectionState.Command.LaunchMissile, new Vector2Int(21, 9));
        }

        {
            yield return WaitForState<SelectionState>();

            var rocketeer = level.units[new Vector2Int(43, -7)];
            var missileSilo = level.buildings[new Vector2Int(43, -5)];

            yield return WaitForState<SelectionState>();
            game.EnqueueCommand(SelectionState.Command.Select, rocketeer.NonNullPosition);

            yield return WaitForState<PathSelectionState>();
            game.EnqueueCommand(PathSelectionState.Command.ReconstructPath, missileSilo.position);
            game.EnqueueCommand(PathSelectionState.Command.Move);

            yield return WaitForState<ActionSelectionState>();
            game.EnqueueCommand(ActionSelectionState.Command.Execute, new UnitAction(UnitActionType.LaunchMissile, rocketeer, stateMachine.Find<PathSelectionState>().path, targetBuilding: missileSilo));

            yield return WaitForState<MissileTargetSelectionState>();
            stateMachine.Peek<MissileTargetSelectionState>().AimPosition = new Vector2Int(21, 9);

            yield return Wait(1);
            game.EnqueueCommand(MissileTargetSelectionState.Command.LaunchMissile, new Vector2Int(21, 9));
        }

        yield return WaitForState<SelectionState>();
        stateMachine.Push(new BorderIncidentWhatIsHappeningDialogueState(stateMachine) { blueInfantry = blueInfantry });
        while (stateMachine.TryFind<BorderIncidentWhatIsHappeningDialogueState>() != null)
            yield return null;
    }
}

public class BorderIncidentIntroDialogueState : DialogueState {
    public BorderIncidentIntroDialogueState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Start();
            yield return AddPerson(PersonName.BlueSoldier, DialogueUi4.Side.Left);
            yield return AddPerson(PersonName.RedSoldier, DialogueUi4.Side.Right);

            var voiceLines = "Border".LoadAs<VoiceLinesSequence>().clips;
            var index = 0;

            Speaker = PersonName.BlueSoldier;
            yield return SayWait(_("It is a good morning, isn't it?"), voiceOver: voiceLines[index++]);
            Speaker = PersonName.RedSoldier;
            yield return Say(_("Well, it was until you showed up."), voiceOver: voiceLines[index++]);
            while (VoiceOver.IsPlaying)
                yield return StateChange.none;
            yield return AppendWait(_(" It is going to be a good morning for me, at least."), voiceOver: voiceLines[index++]);
            Speaker = PersonName.BlueSoldier;
            yield return Say(_("Grumpy as always, I see."), voiceOver: voiceLines[index++]);
            while (VoiceOver.IsPlaying)
                yield return StateChange.none;
            yield return AppendWait(_(" I was ordered to patrol this area. I am not here to bother you."), voiceOver: voiceLines[index++]);
            Speaker = PersonName.RedSoldier;
            yield return SayWait(_("I am not here to be bothered by you."), voiceOver: voiceLines[index++]);
            Speaker = PersonName.BlueSoldier;
            yield return SayWait(_("Nice talking to you, as always."), voiceOver: voiceLines[index++]);

            yield return RemovePerson(DialogueUi4.Side.Left);
            yield return RemovePerson(DialogueUi4.Side.Right);
            End();
        }
    }
}

public class BorderIncidentRedRocketeersDialogueState : DialogueState {
    public BorderIncidentRedRocketeersDialogueState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Start();
            yield return AddPerson(PersonName.RedRocketeer, DialogueUi4.Side.Left, flipped: true);
            yield return AddPerson(PersonName.RedSoldier, DialogueUi4.Side.Right);

            var voiceLines = "Rocketeers".LoadAs<VoiceLinesSequence>().clips;
            var index = 0;

            Speaker = PersonName.RedRocketeer;
            yield return SayWait(_("I think it is about time?"), voiceOver: voiceLines[index++]);
            yield return Say(_("Yeah, I think so..."), voiceOver: voiceLines[index++]);
            yield return Wait(1);
            yield return AppendWait(_("\nHQ, HQ, this is Hornet's Nest. We are ready to engage. Do you copy?"), voiceOver: voiceLines[index++]);
            yield return SayWait(_("This is HQ. We copy. Engage at will."), voiceOver: voiceLines[index++]);
            yield return Say(_("Roger that..."), voiceOver: voiceLines[index++]);
            yield return Wait(1);
            yield return AppendWait(_(" It's show time!"), voiceOver: voiceLines[index++]);

            yield return RemovePerson(DialogueUi4.Side.Left);
            yield return RemovePerson(DialogueUi4.Side.Right);
            End();
        }
    }
}

public class BorderIncidentWhatIsHappeningDialogueState : DialogueState {
    public Unit blueInfantry;
    public BorderIncidentWhatIsHappeningDialogueState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var voiceOvers = "BorderAfter".LoadAs<VoiceLinesSequence>().clips;
            var index = 0;

            /* var cameraRig = stateMachine.Find<LevelSessionState>().level.view.cameraRig;
             if (blueInfantry.NonNullPosition.TryRaycast(out var hit)) {
                 var jump = cameraRig.JumpAnimation(hit.point);
                 while (jump.MoveNext())
                     yield return StateChange.none;
             }*/

            Start();
            yield return AddPerson(PersonName.BlueSoldier, DialogueUi4.Side.Left);

            Speaker = PersonName.BlueSoldier;
            yield return Say(_("We are under attack!"), voiceOver: voiceOvers[index++]);
            yield return Wait(1);
            yield return AppendWait(_(" We need to get out of here!"), voiceOver: voiceOvers[index++]);
            yield return SayWait(_("HQ! HQ! This is Blue Eagle! We are under attack! The enemy has reinforcements! Do you copy?"), voiceOver: voiceOvers[index++]);

            /*if (new Vector2Int(34, 9).TryRaycast(out hit)) {
                var jump = cameraRig.JumpAnimation(hit.point);
                while (jump.MoveNext())
                    yield return StateChange.none;
            }*/

            var redPlayer = stateMachine.Find<LevelSessionState>().level.players.Single(p => p.ColorName == ColorName.Red);
            //var redRocketTop = new Unit(redPlayer, UnitType.Rockets, new Vector2Int(32, 10));
            //var redRocketBottom = new Unit(redPlayer, UnitType.Rockets, new Vector2Int(32, 7));
            //var redLightTankTop = new Unit(redPlayer, UnitType.LightTank, new Vector2Int(35, 10));
            //var redLightTankBottom = new Unit(redPlayer, UnitType.LightTank, new Vector2Int(35, 7));

            //yield return SayWait(_("They have reinforcements!") , voiceOver: voiceOvers[index++]);
            yield return RemovePerson(DialogueUi4.Side.Left);

            yield return AddPerson(PersonName.RedSoldier, DialogueUi4.Side.Right);
            Speaker = PersonName.RedSoldier;
            yield return SayWait(_("A good day indeed."));

            yield return RemovePerson(DialogueUi4.Side.Right);
            End();
        }
    }
}