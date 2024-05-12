using System.Linq;
using System.Text;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;

public class AiTestBed : MonoBehaviour {

    public Unit unit;
    
    public void OnGUI() {
        var game = Game.Instance;
        var levelSession = game.stateMachine.TryFind<LevelSessionState>();
        if (levelSession == null)
            return;

        GUI.skin = DefaultGuiSkin.TryGet;
        var level = levelSession.level;

        var camera = level.view.cameraRig.camera;
        if (camera.TryPhysicsRaycast(out Vector3 point, LayerMasks.Terrain) || Mouse.TryRaycastPlane(camera, out point, 0)) {
            var position = point.ToVector2Int();
            if (level.TryGetUnit(position, out unit) && unit.Player.IsAi) {
                string text;
                if (unit.aiStates.Count > 0) {
                    var sb = new StringBuilder();
                    foreach (var state in unit.aiStates) {
                        sb.Append(state.GetType().Name);
                        sb.AppendLine(":");
                        sb.AppendLine(state.ToString());
                        sb.AppendLine();
                    }
                    text = sb.ToString();
                }
                else
                    text = "<empty>";
                var size = GUI.skin.label.CalcSize(new GUIContent(text));
                GUI.Label(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, size.x, size.y), text);
            }
        }
        else
            unit = null;
    }

    [Command]
    public void PushStayState() {
        if (unit == null)
            return;
        
    }
}

public class UnitAiState {
    public Unit unit;
}

public class UnitAiStayingState : UnitAiState {
    public Vector2Int position;
}

public class UnitAiHealingState : UnitAiState {
    public Building building;
}

public class UnitAiEngagingState : UnitAiState {
    public Unit target;
}