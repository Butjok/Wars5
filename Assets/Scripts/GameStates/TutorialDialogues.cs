using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;
using static Gettext;

public class TutorialStartDialogue : DialogueState {

    public TutorialStartDialogue(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
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
            cameraRig.enabledMovements = CameraRig.MovementType.None;
            
            yield return AddPerson(PersonName.Natalie, DialogueUi4.Side.Left);
            Speaker = PersonName.Natalie;
            yield return SayWait(_("Welcome to the Blue Army Commander!"));
            yield return SayWait(_("My name is Natalie, and I will be your guide through this tutorial."));
            yield return SayWait(_("I will help you get started with the basics of the game."));
            yield return SayWait(_("In turns you will be able to move your units, capture buildings, and attack enemy units."));

            cameraRig.enabledMovements = CameraRig.MovementType.Wasd | CameraRig.MovementType.Rotate;
            ui.wasdImage.enabled = true;
            yield return SayWait(_("Move the camera around using WASD keys and rotate it using Q and E keys. Try it now!"));
            ui.wasdImage.enabled = false;
            
            cameraRig.enabledMovements |= CameraRig.MovementType.Pitch | CameraRig.MovementType.Zoom | CameraRig.MovementType.Drag;
            ui.movement2.enabled = true;
            yield return SayWait(_("To change the elevation use 1 and 2 keys.\nZoom in and out using the mouse wheel.\nYou can also hold the middle mouse button move around that way.\nDouble click with middle mouse button to jump to a location."));
            ui.movement2.enabled = false;
            
            cameraRig.enabledMovements = CameraRig.MovementType.None;
            
            if (hqs[player].position.TryRaycast(out var hit)) {
                var completed = cameraRig.Jump(hit.point);
                while (!completed())
                    yield return StateChange.none;
                using (ui.PulsateCircle(hit.point)) {
                    cameraRig.enabledMovements = CameraRig.MovementType.FixedInPosition;
                    yield return SayWait(_("This is your base, the HQ building.\nIf it is captured, you lose the game.\nIf you capture the enemy HQ, you win the game."));
             
                    if (hqs[enemy].position.TryRaycast(out hit)) {
                         completed = cameraRig.Jump(hit.point);
                        while (!completed())
                            yield return StateChange.none;
                        using (ui.PulsateCircle(hit.point))
                            yield return SayWait(_("Here is the enemy HQ."));
                    }
                    
                    cameraRig.enabledMovements = CameraRig.MovementType.None;
                }
            }
            
            if (infantryUnit.NonNullPosition.TryRaycast(out hit)) {
                var completed = cameraRig.Jump(hit.point);
                while (!completed())
                    yield return StateChange.none;
                using (ui.PulsateCircle(hit.point)) {
                    yield return SayWait(_("Now please select this infantry unit with a left click."));
                }
            }
            
            //yield return SayWait(_("When you want to end your turn, click the End Turn button or F2."));

            /*if (true) {
                yield return Say(_("Do you want to watch tutorial?"));
                bool yes = default;
                yield return ChooseYesNo(value => yes = value);
                if (yes) {
                    yield return Say(_("Sure thing!"));
                    yield return Wait(.5f);
                    yield return SayWait(_("Let us start with the basics!"));
                    yield return ShowVideoPanel();
                    yield return Wait(.25f);
                    MakeDark();
                    Time.timeScale = .25f;
                    var video = CreateVideo("unit-movement".LoadAs<VideoClip>(), target: VideoPanelImage);                    
                    video.player.playbackSpeed = 1;
                    yield return WaitWhile(() => !video.Completed);
                    yield return SayWait(_("Now that you know the basics, let us get started!"));
                    DestroyVideo(video);
                    yield return Wait(.25f);
                    Time.timeScale = 1;
                    MakeLight();
                    yield return Wait(.25f);
                    yield return HideVideoPanel();
                }
                else
                    yield return SayWait(_("Sure thing, let us get started!"));
            }*/

            yield return RemovePerson(DialogueUi4.Side.Left);
            End();
            cameraRig.enabledMovements = CameraRig.MovementType.All;
        }
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