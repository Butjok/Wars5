using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public class MissileTargetSelectionState : StateMachineState {

    public enum Command {
        LaunchMissile,
        Cancel
    }

    public MissileTargetSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public static Easing.Name easing = Easing.Name.InOutQuad;

    [Command] public static Color color = new Color(1f, 0.92f, 0.02f, 0.33f);

    public override IEnumerator<StateChange> Enter {
        get {
            var action = stateMachine.Find<ActionSelectionState>().selectedAction;

            Assert.AreEqual(TileType.MissileSilo, action.targetBuilding);
            Assert.IsTrue(Rules.CanLaunchMissile(action.unit, action.targetBuilding));

            var building = action.targetBuilding;
            var missileSiloView = (MissileSiloView)building.view;

            Vector2Int? launchPosition = null;

            var targets = new List<(Unit target, int damage)>();
            Game.SetGui("missile-targets", () => {
                foreach (var (target, damage) in targets)
                    WarsGui.CenteredLabel(Level, target.view.body.position, $"-{damage}");
            });

            while (true) {
                yield return StateChange.none;

                if (Input.GetMouseButtonDown(Mouse.left) && Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition)) {
                    if ((mousePosition - building.position).ManhattanLength().IsInRange(building.missileSilo.range)) {
                        if (launchPosition != mousePosition) {
                            launchPosition = mousePosition;

                            targets.Clear();
                            foreach (var unit in Level.Units) {
                                // unit might make a move to the missile silo and fire the missile from there
                                // therefore we need to behave like unit's position is at the missile silo position
                                var unitPosition = unit == action.unit ? building.position : unit.NonNullPosition;
                                var distance = (unitPosition - mousePosition).ManhattanLength();
                                if (distance.IsInRange(building.missileSilo.blastRange))
                                    targets.Add((unit, building.missileSilo.unitDamage));
                            }
                        }
                        else
                            Game.EnqueueCommand(Command.LaunchMissile, launchPosition);
                    }
                    else
                        UiSound.Instance.notAllowed.PlayOneShot();
                }

                else if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    Game.EnqueueCommand(Command.Cancel);

                while (Game.TryDequeueCommand(out var command))
                    switch (command) {
                        case (Command.LaunchMissile, Vector2Int targetPosition): {
                            Assert.AreEqual(TileType.MissileSilo, building.type);

                            targets.Clear();

                            Debug.Log($"Launching missile from {building.position} to {targetPosition}");
                            /*using (Draw.ingame.WithDuration(1))
                            using (Draw.ingame.WithLineWidth(2))
                                Draw.ingame.Arrow((Vector3)missileSilo.position.ToVector3Int(), (Vector3)targetPosition.ToVector3Int(), Vector3.up, .25f, Color.red);*/

                            action.unit.Position = action.path[^1];

                            if (missileSiloView) {
                                missileSiloView.SnapToTargetRotationInstantly();

                                var cameraRig = Level.view.cameraRig;
                                cameraRig.enabled = false;

                                var jumpCompleted = cameraRig.Jump(missileSiloView.transform.position.ToVector2().ToVector3());
                                while (!jumpCompleted())
                                    yield return StateChange.none;
                                
                                building.missileSilo.lastLaunchDay = Level.Day();
                                building.missileSilo.ammo--;
                                building.missileSilo.hasMissile = false;
                                building.Moved = true;

                                var missile = missileSiloView.TryLaunchMissile(building.Player.Color);
                                Assert.IsTrue(missile);
                                if (missile.curve.totalTime is not { } flightTime)
                                    throw new AssertionException("missile.curve.totalTime = null", null);

                                Level.RemoveGui("tilemap-cursor");

                                var startTime = Time.time;
                                var startPosition = cameraRig.transform.position.ToVector2();
                                while (Time.time < startTime + flightTime) {
                                    var t = (Time.time - startTime) / flightTime;
                                    t = Easing.Dynamic(easing, t);
                                    cameraRig.transform.position = Vector2.Lerp(startPosition, targetPosition, t).ToVector3();
                                    yield return StateChange.none;
                                }

                                Sounds.PlayOneShot(Sounds.explosion);
                                cameraRig.Shake(.25f);
                                for (var radius = building.missileSilo.blastRange[0]; radius <= building.missileSilo.blastRange[1]; radius++) {
                                    foreach (var position in Level.PositionsInRange(targetPosition, new Vector2Int(radius, radius))) {
                                        if (Level.TryGetUnit(position, out var unit)) {
                                            unit.SetHp(unit.Hp - building.missileSilo.unitDamage, true);
                                            if (unit.Hp > 0) {
                                                unit.view.ApplyDamageTorque(UnitView.DamageTorqueType.MissileExplosion);
                                                unit.view.TriggerDamageFlash();
                                            }
                                        }

                                        if (position.TryRaycast(out var hit))
                                            Effects.SpawnExplosion(hit.point);
                                    }

                                    startTime = Time.time;
                                    while (Time.time < startTime + .05f)
                                        yield return StateChange.none;
                                }

                                cameraRig.enabled = true;
                            }

                            var anyBridgeDestroyed = false;
                            
                            if (Level.TryGetBridge2(targetPosition, out var bridge)) {
                                bridge.Hp -= 5;
                                if (bridge.Hp <= 0)
                                    anyBridgeDestroyed = true;
                            }

                            missileSiloView.aim = false;

                            yield break;
                        }

                        case (Command.Cancel, _):
                            missileSiloView.aim = false;
                            // pop MissileTargetSelectionState and current ActionSelectionState -> then restart ActionSelectionState
                            // it is important that the ActionSelectionState is a IDisposableState in order to dispose
                            // UnitAction list properly
                            yield return StateChange.PopThenPush(2, new ActionSelectionState(stateMachine));
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                if (launchPosition is { } actualLaunchPosition)
                    foreach (var attackPosition in Level.PositionsInRange(actualLaunchPosition, building.missileSilo.blastRange)) {
                        var point = attackPosition.TryRaycast(out var hit) ? hit.point : attackPosition.ToVector3();
                        Draw.ingame.SolidPlane(point, Vector3.up, Vector2.one, color);
                    }

                {
                    if (AimPosition is {} actualAimPosition) {
                        missileSiloView.aim = true;
                        if (actualAimPosition.TryRaycast(out var hit2)) {
                            missileSiloView.targetPosition = hit2.point;
                            /*if (missileSiloView.TryCalculateCurve(out var curve))
                                using (Draw.ingame.WithLineWidth(1.5f))
                                    foreach (var (start, end) in curve.Segments())
                                        Draw.ingame.Line(start, end, Color.red);*/
                        }
                    }
                    else
                        missileSiloView.aim = false;
                }
            }
        }
    }

    private Vector2Int? aimPosition;
    public Vector2Int? AimPosition {
        get {
            if (aimPosition is { } actualAimPosition)
                return actualAimPosition;
            if (Level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition))
                return mousePosition;
            return null;
        }
        set => aimPosition = value;
    }

    public override void Exit() {
        base.Exit();
        var game = stateMachine.Find<GameSessionState>().game;
        game.RemoveGui("missile-targets");
    }
}