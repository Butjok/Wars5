using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public class MissileTargetSelectionState : StateMachineState {

    public enum Command { LaunchMissile, Cancel }

    public MissileTargetSelectionState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            var (game, level, action) = (FindState<GameSessionState>().game, FindState<LevelSessionState>().level, FindState<ActionSelectionState>().selectedAction);

            Assert.AreEqual(TileType.MissileSilo, action.targetBuilding);
            Assert.IsTrue(Rules.CanLaunchMissile(action.unit, action.targetBuilding));

            var missileSilo = action.targetBuilding;
            var missileSiloView = (MissileSiloView)missileSilo.view;

            Vector2Int? launchPosition = null;

            while (true) {
                yield return StateChange.none;

                if (Input.GetMouseButtonDown(Mouse.left) && level.view.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition)) {
                    if ((mousePosition - missileSilo.position).ManhattanLength().IsInRange(missileSilo.missileSiloRange)) {
                        if (launchPosition != mousePosition)
                            launchPosition = mousePosition;
                        else
                            game.EnqueueCommand(Command.LaunchMissile, launchPosition);
                    }
                    else
                        UiSound.Instance.notAllowed.PlayOneShot();
                }

                else if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                    game.EnqueueCommand(Command.Cancel);

                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (Command.LaunchMissile, Vector2Int targetPosition): {

                            Assert.AreEqual(TileType.MissileSilo, missileSilo.type);

                            Debug.Log($"Launching missile from {missileSilo.position} to {targetPosition}");
                            using (Draw.ingame.WithDuration(1))
                            using (Draw.ingame.WithLineWidth(2))
                                Draw.ingame.Arrow((Vector3)missileSilo.position.ToVector3Int(), (Vector3)targetPosition.ToVector3Int(), Color.red);

                            if (missileSiloView) {

                                missileSiloView.SnapToTargetRotationInstantly();

                                var missile = missileSiloView.TryLaunchMissile();
                                Assert.IsTrue(missile);
                                if (missile.curve.totalTime is not { } flightTime)
                                    throw new AssertionException("missile.curve.totalTime = null", null);

                                var cameraRig = level.view.cameraRig;
                                cameraRig.Jump(Vector2.Lerp(missileSilo.position, targetPosition, .5f).Raycast());
                                var startTime = Time.time;
                                while (Time.time < startTime + flightTime)
                                    yield return StateChange.none;
                            }

                            action.unit.Position = action.path[^1];
                            missileSilo.missileSiloLastLaunchTurn = level.turn;
                            missileSilo.missileSiloAmmo--;

                            foreach (var position in level.PositionsInRange(targetPosition, missileSilo.missileBlastRange))
                                if (level.TryGetUnit(position, out var unit))
                                    unit.SetHp(unit.Hp - missileSilo.missileUnitDamage, true);

                            var anyBridgeDestroyed = false;
                            var targetedBridges = level.bridges.Where(bridge => bridge.tiles.ContainsKey(targetPosition));
                            foreach (var bridge in targetedBridges) {
                                bridge.SetHp(bridge.Hp - missileSilo.missileBridgeDamage, true);
                                if (!anyBridgeDestroyed && bridge.Hp <= 0)
                                    anyBridgeDestroyed = true;
                            }

                            missileSiloView.aim = false;

                            if (anyBridgeDestroyed) {
                                using var dialogue = new DialoguePlayer();
                                foreach (var stateChange in dialogue.Play(@"
@nata Hello there! @next
      Welcome to the Wars3d! An amazing strategy game! @next

@vlad What are you saying? @next

@nata I dont know what to say... @next
@nervous Probably... @3 @pause we should have done something different... @next

@nata @happy You probably did not know who you are messing with! @next
@nata @normal Enough said. @next
"))
                                    yield return stateChange;

                                var cameraRig = level.view.cameraRig;
                                cameraRig.Jump(Vector3.zero);
                                while (cameraRig.JumpCoroutine != null)
                                    yield return StateChange.none;

                                foreach (var stateChange in dialogue.Play(@"
@nata So... @1 @pause Here it goes! @next
"))
                                    yield return stateChange;
                            }

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
                    foreach (var attackPosition in level.PositionsInRange(actualLaunchPosition, missileSilo.missileBlastRange))
                        Draw.ingame.SolidPlane((Vector3)attackPosition.ToVector3Int(), Vector3.up, Vector2.one, Color.red);

                if (level.view.cameraRig.camera.TryGetMousePosition(out var hit, out mousePosition) && missileSiloView &&
                    (mousePosition - missileSilo.position).ManhattanLength().IsInRange(missileSilo.missileSiloRange)) {

                    missileSiloView.aim = true;
                    missileSiloView.targetPosition = hit.point;
                }
                else
                    missileSiloView.aim = false;
            }
        }
    }
}