using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class AiTestBed : MonoBehaviour {

    public Camera camera;

    public Unit selectedUnit;
    public UnitState selectedState;

    public void Update() {
        using (Draw.ingame.WithLineWidth(1f)) {
            var game = Game.Instance;
            var level = game.stateMachine.TryFind<LevelSessionState>()?.level ?? game.stateMachine.TryFind<LevelEditorSessionState>()?.level;
            if (level == null)
                return;

            if (selectedUnit != null && (!selectedUnit.Initialized || selectedUnit.Player.level != level)) {
                selectedUnit = null;
                selectedState = null;
            }

            if (camera.TryGetMousePosition(out Vector2Int mousePosition)) {
                Draw.ingame.CircleXZ(mousePosition.Raycasted(), .5f, Color.yellow);
                if (Input.GetKeyDown(KeyCode.I)) {
                    if (level.TryGetUnit(mousePosition, out var unit)) {
                        selectedUnit = unit;
                        selectedState = unit.states2.Count > 0 ? unit.states2[^1] : null;
                    }
                    else {
                        selectedUnit = null;
                        selectedState = null;
                    }
                }
            }

            if (selectedUnit != null) {
                if (Input.GetKeyDown(KeyCode.O)) {
                    var list = selectedUnit.states2.ToList();
                    var index = list.IndexOf(selectedState);
                    var nextIndex = (index + 1) % list.Count;
                    selectedState = list[nextIndex];
                }
                if (selectedUnit.Position is { } actualPosition)
                    Draw.ingame.CircleXZ(actualPosition.Raycasted(), .5f, Color.white);
            }

            var units = selectedUnit != null ? new[] { selectedUnit } : level.Units;
            foreach (var unit in units)
            foreach (var state in unit.states2) {
                if (state.HasExpired)
                    continue;

                var text = state.GetType().ToString();
                if (text.EndsWith("State"))
                    text = text[..^5];
                if (text.StartsWith("Unit"))
                    text = text[4..];
                text += $"\n{state.DaysLeft}";

                Color color;
                var fade = state == selectedState ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, .125f);

                void DrawLine(Vector2Int from, Vector2Int to) {
                    Draw.ingame.Line(from.Raycasted(), to.Raycasted(), color * fade);
                }

                void DrawLabel(Vector2Int position, string text) {
                    Draw.ingame.Label3D(position.Raycasted(), Quaternion.LookRotation(Vector3.down), text, (float)0.25, LabelAlignment.Center, color * fade);
                }

                void DrawCircle(Vector2Int position) {
                    Draw.ingame.CircleXZ(position.Raycasted(), .5f, color * fade);
                }

                switch (state) {
                    case UnitMoveState moveGoal: {
                        color = Color.green;
                        var movePosition = moveGoal.position;
                        DrawCircle(movePosition);
                        DrawLabel(movePosition, text);
                        if (unit.Position is { } unitPosition)
                            DrawLine(unitPosition, movePosition);
                        break;
                    }

                    case UnitKillState killGoal: {
                        color = Color.red;
                        var target = killGoal.target;
                        if (target.Initialized && target.Position is { } targetPosition) {
                            DrawCircle(targetPosition);
                            DrawLabel(targetPosition, text);
                            if (unit.Position is { } unitPosition)
                                DrawLine(unitPosition, targetPosition);
                        }
                        break;
                    }

                    case UnitCaptureState captureGoal: {
                        color = Color.cyan;
                        var buildingPosition = captureGoal.building.position;
                        DrawCircle(buildingPosition);
                        DrawLabel(buildingPosition, text);
                        if (unit.Position is { } unitPosition)
                            DrawLine(unitPosition, buildingPosition);
                        break;
                    }

                    case UnitTransferState transferGoal: {
                        color = Color.blue;
                        var pickUpUnit = transferGoal.pickUpUnit;
                        var dropPosition = transferGoal.dropPosition;
                        DrawCircle(dropPosition);
                        DrawLabel(dropPosition, text);
                        if (unit.Position is { } unitPosition) {
                            if (pickUpUnit.Position is { } pickUpUnitPosition) {
                                DrawLine(unitPosition, pickUpUnitPosition);
                                DrawLine(pickUpUnitPosition, dropPosition);
                            }
                            else
                                DrawLine(unitPosition, dropPosition);
                        }
                        break;
                    }

                    /*case UnitHealState healState: {
                        color = Color.magenta;
                        var target = healState;
                        if (target.Initialized && target.Position is { } targetPosition) {
                            DrawCircle(targetPosition);
                            DrawLabel(targetPosition, text);
                            if (unit.Position is { } unitPosition)
                                DrawLine(unitPosition, targetPosition);
                        }
                        break;
                    }*/
                }
            }
        }
    }

    public void OnGUI() {
        if (selectedUnit == null)
            return;
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Space(175);
        for (var i = selectedUnit.states2.Count - 1; i >= 0; i--) {
            var state = selectedUnit.states2[i];
            var text = state.ToString();
            GUILayout.Label(selectedState == state ? $"<b>{text}</b>" : text);
            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        }
    }

    [Command]
    public void PushMoveGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition)) {
            var goal = new UnitMoveState {
                unit = selectedUnit,
                createdOnDay = selectedUnit.Player.level.Day(),
                position = mousePosition
            };
            selectedUnit.states2.Add(goal);
            selectedState = goal;
        }
    }

    [Command]
    public void ClearGoals() {
        if (selectedUnit != null) {
            if (selectedUnit.states2.Contains(selectedState))
                selectedState = null;
            selectedUnit.states2.Clear();
        }
    }

    [Command]
    public void PushKillGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition) &&
            selectedUnit.Player.level.TryGetUnit(mousePosition, out var target) && Rules.AreEnemies(selectedUnit.Player, target.Player)) {
            var goal = new UnitKillState {
                unit = selectedUnit,
                createdOnDay = selectedUnit.Player.level.Day(),
                target = target
            };
            selectedUnit.states2.Add(goal);
            selectedState = goal;
        }
    }

    [Command]
    public void PushCaptureGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition) &&
            selectedUnit.Player.level.TryGetBuilding(mousePosition, out var building) && Rules.CanCapture(selectedUnit.type, building.type)) {
            var goal = new UnitCaptureState {
                unit = selectedUnit,
                createdOnDay = selectedUnit.Player.level.Day(),
                building = building
            };
            selectedUnit.states2.Add(goal);
            selectedState = goal;
        }
    }

    [Command]
    public void ClearUnitStates() {
        var level = Game.Instance.TryGetLevel;
        if (level != null)
            foreach (var unit in level.Units)
                unit.states2.Clear();
    }

    [Command]
    public void SetHp(int hp) {
        hp = Mathf.Max(1, hp);
        if (selectedUnit != null)
            selectedUnit.Hp = hp;
    }

    [Command]
    public void PushTransferGoal() {
        if (selectedUnit != null)
            StartCoroutine(TransferGoalSelection());
    }
    public IEnumerator TransferGoalSelection() {
        Unit pickUpUnit = null;
        while (true) {
            yield return null;
            if (Input.GetKeyDown(KeyCode.Escape))
                yield break;
            if (camera.TryGetMousePosition(out Vector2Int mousePosition) &&
                selectedUnit.Player.level.TryGetUnit(mousePosition, out var other) && other != selectedUnit && Rules.CanGetIn(other, selectedUnit) &&
                Input.GetKeyDown(KeyCode.P)) {
                pickUpUnit = other;
                break;
            }
        }
        while (true) {
            yield return null;
            if (camera.TryGetMousePosition(out Vector2Int mousePosition) &&
                selectedUnit.Player.level.TryGetTile(mousePosition, out var tileType) &&
                Rules.CanStay(pickUpUnit, tileType) &&
                Input.GetKeyDown(KeyCode.P)) {
                selectedUnit.states2.Add(new UnitTransferState {
                    unit = selectedUnit,
                    createdOnDay = selectedUnit.Player.level.Day(),
                    pickUpUnit = pickUpUnit,
                    dropPosition = mousePosition
                });
                break;
            }
        }
    }
}