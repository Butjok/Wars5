using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class AiTestBed : MonoBehaviour {

    public Camera camera;

    public Unit selectedUnit;
    public UnitAiState selectedState;

    public void Update() {
        using (Draw.ingame.WithLineWidth(1.75f)) {
            if (selectedUnit is { Initialized: false }) {
                selectedUnit = null;
                selectedState = null;
            }
            var game = Game.Instance;
            var levelSessionState = game.stateMachine.TryFind<LevelSessionState>();
            if (levelSessionState == null)
                return;
            var level = levelSessionState.level;
            if (camera.TryGetMousePosition(out Vector2Int position)) {
                Draw.ingame.CircleXZ(position.Raycasted(), .5f, Color.yellow);
                if (Input.GetKeyDown(KeyCode.I) && level.TryGetUnit(position, out var unit)) {
                    selectedUnit = unit;
                    selectedState = unit.aiStates.Count > 0 ? unit.aiStates.Peek() : null;
                }
            }
            if (Input.GetMouseButtonDown(Mouse.right)) {
                selectedUnit = null;
                selectedState = null;
            }
            if (selectedUnit != null) {
                if (Input.GetKeyDown(KeyCode.O)) {
                    var list = selectedUnit.aiStates.ToList();
                    var index = list.IndexOf(selectedState);
                    var nextIndex = (index + 1) % list.Count;
                    selectedState = list[nextIndex];
                }
                if (selectedUnit.Position is { } actualPosition)
                    Draw.ingame.CircleXZ(actualPosition.Raycasted(), .5f, Color.white);
                if (selectedState != null)
                    foreach (var state in selectedUnit.aiStates) {
                        var fade = state == selectedState ? Color.white : new Color(1, 1, 1, .25f);
                        switch (state) {
                            case UnitAiStayingState stayingState:
                                Draw.ingame.CircleXZ(stayingState.position.Raycasted(), .5f, Color.green * fade);
                                Draw.ingame.Label3D(stayingState.position.Raycasted(), Quaternion.LookRotation(Vector3.down), "Stay", (float)0.25, LabelAlignment.Center, Color.green * fade);
                                if (selectedUnit.Position is { } actualPosition2)
                                    Draw.ingame.Line(actualPosition2.Raycasted(), stayingState.position.Raycasted(), Color.green * fade);
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
        foreach (var state in selectedUnit.aiStates) {
            var text = state.ToString();
            GUILayout.Label(selectedState == state ? $"<b>{text}</b>" : text);
            GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
        }
    }

    [Command]
    public void PushStayState() {
        if (selectedUnit == null)
            return;
        if (camera.TryGetMousePosition(out Vector2Int position)) {
            selectedState = new UnitAiStayingState {
                unit = selectedUnit
            };
            selectedUnit.aiStates.Push(selectedState);
        }
    }
}

public class UnitAiState {
    public Unit unit;
}
public class UnitAiStayingState : UnitAiState {
    public List<Vector2Int> area = new();
    public Vector2Int position;
}
public class UnitAiHealingState : UnitAiState {
    public Building building;
}
public class UnitAiEngagingState : UnitAiState {
    public Unit target;
}
public class UnitAiCaptureBuilding : UnitAiState {
    public Building building;
}
public class UnitAiGetInApc : UnitAiState {
    public Unit apc;
}
public class UnitAiPickUpUnit : UnitAiState {
    public Unit unitToPickUp;
}
public class UnitAiDropUnit : UnitAiState {
    public List<Vector2Int> area = new();
}