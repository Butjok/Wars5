using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class AiTestBed : MonoBehaviour {

    public Camera camera;

    public Unit selectedUnit;
    public UnitGoal selectedGoal;

    public void Update() {
        using (Draw.ingame.WithLineWidth(1.75f)) {
            var game = Game.Instance;
            var level = game.stateMachine.TryFind<LevelSessionState>()?.level ?? game.stateMachine.TryFind<LevelEditorSessionState>()?.level;
            if (level == null)
                return;

            if (selectedUnit != null && (!selectedUnit.Initialized || selectedUnit.Player.level != level)) {
                selectedUnit = null;
                selectedGoal = null;
            }

            if (camera.TryGetMousePosition(out Vector2Int mousePosition)) {
                Draw.ingame.CircleXZ(mousePosition.Raycasted(), .5f, Color.yellow);
                if (Input.GetKeyDown(KeyCode.I)) {
                    if (level.TryGetUnit(mousePosition, out var unit)) {
                        selectedUnit = unit;
                        selectedGoal = unit.goals.Count > 0 ? unit.goals.Peek() : null;
                    }
                    else {
                        selectedUnit = null;
                        selectedGoal = null;
                    }
                }
            }

            if (selectedUnit != null) {
                if (Input.GetKeyDown(KeyCode.O)) {
                    var list = selectedUnit.goals.ToList();
                    var index = list.IndexOf(selectedGoal);
                    var nextIndex = (index + 1) % list.Count;
                    selectedGoal = list[nextIndex];
                }
                if (selectedUnit.Position is { } actualPosition)
                    Draw.ingame.CircleXZ(actualPosition.Raycasted(), .5f, Color.white);
            }

            var units = selectedUnit != null ? new[] { selectedUnit } : level.Units;
            foreach (var unit in units)
            foreach (var goal in unit.goals) {
                var text = goal.GetType().ToString();
                if (text.EndsWith("Goal"))
                    text = text[..^4];
                if (text.StartsWith("Unit"))
                    text = text[4..];

                Color color;
                var fade = goal == selectedGoal ? Color.white : new Color(1, 1, 1, .25f);

                void DrawLine(Vector2Int from, Vector2Int to) {
                    Draw.ingame.Line(from.Raycasted(), to.Raycasted(), color * fade);
                }

                void DrawLabel(Vector2Int position, string text) {
                    Draw.ingame.Label3D(position.Raycasted(), Quaternion.LookRotation(Vector3.down), text, (float)0.25, LabelAlignment.Center, color * fade);
                }

                void DrawCircle(Vector2Int position) {
                    Draw.ingame.CircleXZ(position.Raycasted(), .5f, color * fade);
                }

                switch (goal) {
                    case UnitMoveGoal moveGoal: {
                        color = Color.green;
                        var movePosition = moveGoal.position;
                        DrawCircle(movePosition);
                        DrawLabel(movePosition, text);
                        if (unit.Position is { } unitPosition)
                            DrawLine(unitPosition, movePosition);
                        break;
                    }

                    case UnitKillGoal killGoal: {
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

                    case UnitCaptureGoal captureGoal: {
                        color = Color.cyan;
                        var buildingPosition = captureGoal.building.position;
                        DrawCircle(buildingPosition);
                        DrawLabel(buildingPosition, text);
                        if (unit.Position is { } unitPosition)
                            DrawLine(unitPosition, buildingPosition);
                        break;
                    }

                    case UnitTransferGoal transferGoal: {
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
                }
            }
        }
    }

    public void OnGUI() {
        if (selectedUnit == null)
            return;
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Space(175);
        foreach (var state in selectedUnit.goals) {
            var text = state.ToString();
            GUILayout.Label(selectedGoal == state ? $"<b>{text}</b>" : text);
            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        }
    }

    [Command]
    public void PushMoveGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition)) {
            var goal = new UnitMoveGoal { unit = selectedUnit, position = mousePosition };
            selectedUnit.goals.Push(goal);
            selectedGoal = goal;
        }
    }

    [Command]
    public void ClearGoals() {
        if (selectedUnit != null) {
            if (selectedUnit.goals.Contains(selectedGoal))
                selectedGoal = null;
            selectedUnit.goals.Clear();
            selectedUnit.goals.Push(new UnitIdleGoal { unit = selectedUnit });
        }
    }

    [Command]
    public void PushKillGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition) &&
            selectedUnit.Player.level.TryGetUnit(mousePosition, out var target) && Rules.AreEnemies(selectedUnit.Player, target.Player)) {
            var goal = new UnitKillGoal { unit = selectedUnit, target = target };
            selectedUnit.goals.Push(goal);
            selectedGoal = goal;
        }
    }

    [Command]
    public void PushCaptureGoal() {
        if (selectedUnit != null && camera.TryGetMousePosition(out Vector2Int mousePosition) &&
            selectedUnit.Player.level.TryGetBuilding(mousePosition, out var building) && Rules.CanCapture(selectedUnit.type, building.type)) {
            var goal = new UnitCaptureGoal { unit = selectedUnit, building = building };
            selectedUnit.goals.Push(goal);
            selectedGoal = goal;
        }
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
                selectedUnit.goals.Push(new UnitTransferGoal {
                    unit = selectedUnit,
                    pickUpUnit = pickUpUnit,
                    dropPosition = mousePosition
                });
                break;
            }
        }
    }
}