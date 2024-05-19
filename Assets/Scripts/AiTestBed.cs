using System.Collections.Generic;
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
                if (selectedGoal != null)
                    foreach (var goal in selectedUnit.goals) {
                        var fade = goal == selectedGoal ? Color.white : new Color(1, 1, 1, .25f);
                        var text = goal.GetType().ToString();
                        if (text.EndsWith("Goal"))
                            text = text[..^4];
                        if (text.StartsWith("Unit"))
                            text = text[4..];
                        switch (goal) {
                            case UnitMoveGoal moveGoal: {
                                var color = Color.green;
                                var position = moveGoal.position;
                                Draw.ingame.CircleXZ(position.Raycasted(), .5f, color * fade);
                                Draw.ingame.Label3D(position.Raycasted(), Quaternion.LookRotation(Vector3.down), text, (float)0.25, LabelAlignment.Center, color * fade);
                                if (selectedUnit.Position is { } actualPosition2)
                                    Draw.ingame.Line(actualPosition2.Raycasted(), position.Raycasted(), color * fade);
                                break;
                            }
                            case UnitKillGoal killGoal: {
                                var color = Color.red;
                                var target = killGoal.target;
                                if (target.Initialized && target.Position is { } position) {
                                    Draw.ingame.CircleXZ(position.Raycasted(), .5f, color * fade);
                                    Draw.ingame.Label3D(position.Raycasted(), Quaternion.LookRotation(Vector3.down), text, (float)0.25, LabelAlignment.Center, color * fade);
                                    if (selectedUnit.Position is { } actualPosition2)
                                        Draw.ingame.Line(actualPosition2.Raycasted(), position.Raycasted(), color * fade);
                                }
                                break;
                            }
                            case UnitCaptureGoal captureGoal: {
                                var color = Color.cyan;
                                var position = captureGoal.building.position;
                                Draw.ingame.CircleXZ(position.Raycasted(), .5f, color * fade);
                                Draw.ingame.Label3D(position.Raycasted(), Quaternion.LookRotation(Vector3.down), text, (float)0.25, LabelAlignment.Center, color * fade);
                                if (selectedUnit.Position is { } actualPosition2)
                                    Draw.ingame.Line(actualPosition2.Raycasted(), position.Raycasted(), color * fade);
                                break;
                            }
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
            selectedUnit.Player.level.TryGetBuilding(mousePosition, out var building) && Rules.CanCapture(selectedUnit, building)) {
            var goal = new UnitCaptureGoal { unit = selectedUnit, building = building };
            selectedUnit.goals.Push(goal);
            selectedGoal = goal;
        }
    }
}