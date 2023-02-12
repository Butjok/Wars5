using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public static class MissileTargetSelectionState {

    public const string prefix = "missile-target-selection-state.";
    public const string launchMissile = prefix + "launch-missile";
    public const string cancel = prefix + "cancel";

    public static IEnumerator<StateChange> Run(Main main, UnitAction action, Vector2Int? initialLookDirection = null) {

        Assert.AreEqual(TileType.MissileSilo, action.targetBuilding);
        Assert.IsTrue(Rules.CanLaunchMissile(action.unit, action.targetBuilding));

        var missileSilo = action.targetBuilding;
        var missileSiloView = (MissileSiloView)missileSilo.view;

        Vector2Int? launchPosition = null;

        while (true) {
            yield return StateChange.none;

            if (Input.GetMouseButtonDown(Mouse.left) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {

                if ((mousePosition - missileSilo.position).ManhattanLength().IsIn(missileSilo.missileSiloRange)) {
                    if (launchPosition != mousePosition) {
                        launchPosition = mousePosition;
                    }
                    else {
                        main.stack.Push(launchPosition);
                        main.commands.Enqueue(launchMissile);
                    }
                }
                else
                    UiSound.Instance.notAllowed.PlayOneShot();
            }

            else if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape))
                main.commands.Enqueue(cancel);

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case launchMissile: {

                            var targetPosition = main.stack.Pop<Vector2Int>();
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

                                if (CameraRig.TryFind(out var cameraRig))
                                    cameraRig.Jump(Vector2.Lerp(missileSilo.position, targetPosition, .5f).Raycast());
                                yield return StateChange.Push("MissileFlight", Wait.ForSeconds(flightTime));
                            }

                            action.unit.Position = action.destination;
                            missileSilo.missileSiloLastLaunchTurn = main.turn;
                            missileSilo.missileSiloAmmo--;

                            foreach (var position in main.PositionsInRange(targetPosition, missileSilo.missileBlastRange))
                                if (main.TryGetUnit(position, out var unit))
                                    unit.SetHp(unit.Hp - missileSilo.missileUnitDamage, true);

                            var anyBridgeDestroyed = false;
                            var targetedBridges = main.bridges.Where(bridge => bridge.tiles.ContainsKey(targetPosition));
                            foreach (var bridge in targetedBridges) {
                                bridge.SetHp(bridge.Hp - missileSilo.missileBridgeDamage, true);
                                if (!anyBridgeDestroyed && bridge.Hp <= 0)
                                    anyBridgeDestroyed = true;
                            }
                            
                            missileSiloView.aim = false;

                            if (anyBridgeDestroyed) {
                                using var dialogue = new Dialogue();
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
                            }
                            
                            yield break;
                        }

                        case cancel:
                            missileSiloView.aim = false;
                            // pop MissileTargetSelectionState and current ActionSelectionState -> then restart ActionSelectionState
                            // it is important that the ActionSelectionState is a IDisposableState in order to dispose
                            // UnitAction list properly
                            yield return StateChange.PopThenPush(2, new ActionSelectionState(main, action.unit, action.Path, initialLookDirection));
                            break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }

            if (launchPosition is { } actualLaunchPosition)
                foreach (var attackPosition in main.PositionsInRange(actualLaunchPosition, missileSilo.missileBlastRange))
                    Draw.ingame.SolidPlane((Vector3)attackPosition.ToVector3Int(), Vector3.up, Vector2.one, Color.red);

            if (Mouse.TryGetPosition(out mousePosition) && missileSiloView &&
                (mousePosition - missileSilo.position).ManhattanLength().IsIn(missileSilo.missileSiloRange)) {

                missileSiloView.aim = true;
                missileSiloView.targetPosition = mousePosition.Raycast();
            }
            else
                missileSiloView.aim = false;
        }
    }
}