using System.Collections;
using System.Globalization;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using static PathFinder;

public class ScalarFieldDrawer : MonoBehaviour {

    private static ScalarFieldDrawer instance;
    public static ScalarFieldDrawer Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<ScalarFieldDrawer>();
                if (!instance) {
                    var gameObject = new GameObject(nameof(ScalarFieldDrawer));
                    instance = gameObject.AddComponent<ScalarFieldDrawer>();
                }
            }
            return instance;
        }
    }

    [Command] public string colorScheme = "viridis";

    public static void Draw(ScalarField field) {
        Instance.StopAllCoroutines();
        Instance.StartCoroutine(DrawEnumerator(field));
    }

    public static IEnumerator DrawEnumerator(ScalarField field) {

        var level = Game.Instance.Level;
        Instance.field = field;
        var ramp = Resources.Load<Texture2D>(Instance.colorScheme);

        while (true) {
            if (Input.GetKeyDown(KeyCode.Alpha9)) {
                yield return null;
                Instance.field = null;
                break;
            }
            yield return null;

            foreach (var position in level.tiles.Keys) {
                if (field.Domain.Contains(position)) {
                    var value = field[position];
                    var color = ramp.GetPixelBilinear(1 - value, .5f);
                    if (value >= infinity)
                        color = Color.black;
                    Drawing.Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, color);
                    if (value <= 0 || value >= 1)
                        Drawing.Draw.ingame.Label2D(position.ToVector3(), value >= infinity ? "inf" : $"{value:0.###}", 8, LabelAlignment.Center, color.YiqContrastColor() * new Color(.5f, .5f, .5f, 1));
                }
                else
                    Drawing.Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, Color.black);
            }
        }
    }

    [Command]
    public static void DrawDistances() {
        var inspectedUnit = Game.Instance.stateMachine.Find<LevelEditorUnitsModeState>().InspectedUnit;
        var level = Game.Instance.stateMachine.Find<LevelEditorSessionState>().level;
        if (inspectedUnit != null) {
            var distances = level.tiles.Keys.ToDictionary(
                position => position,
                position => level.precalculatedDistances.TryGetValue((Rules.GetMoveType(inspectedUnit), inspectedUnit.NonNullPosition, position), out var distance)
                    ? distance
                    : infinity);
            Draw(new ScalarField(distances.Keys.Where(position => distances[position] != infinity), position => distances[position]));
        }
    }

    [Command]
    public static void EvaluateAndDraw(string input) {
        Draw(ScalarFieldCalculator.Evaluate(input, Game.Instance));
    }

    public ScalarField field;

    private void OnGUI() {
        if (field == null)
            return;
        GUI.skin = DefaultGuiSkin.TryGet;
        if (Camera.main && Camera.main.TryGetMousePosition(out Vector2Int mousePosition) && field.Domain.Contains(mousePosition)) {
            var position = Input.mousePosition;
            position.y = Screen.height - position.y;
            var text = "";
            text += field[mousePosition] >= infinity ? "inf" : field[mousePosition].ToString("0.###", CultureInfo.InvariantCulture);
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            position.y -= size.y;
            GUI.Label(new Rect(position, size), text);
        }
    }
}