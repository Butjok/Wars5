using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using Unity.Mathematics;
using UnityEngine;
using ScalarField = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, float>;
using VectorField = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, UnityEngine.Vector2>;

public class InfluenceMapDrawer : MonoBehaviour {

    public Vector2Int subdivisions = new(1, 1);
    public Vector2 hueRange = new(.5f, 0);
    public float noiseScale = 1;
    public Color fade = new(1, 1, 1, .25f);
    public IEnumerator coroutine;
    public bool draw = true;

    public Material[] materials = { };
    [ColorUsage(false)] public Color tintColor = new(.5f, .5f, .5f);

    public void Start() {
        foreach (var material in materials)
            material.SetColor("_Color", Color.white);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            draw = !draw;
            foreach (var material in materials)
                material.SetColor("_Color", draw ? tintColor : Color.white);
        }

        if (draw && Game.Instance && Game.Instance.TryGetLevel != null) {
            var level = Game.Instance.TryGetLevel;

            ScalarField UnitInfluence(ColorName colorName) {
                var player = level.players.Single(p => p.ColorName == colorName);
                var units = level.Units.Where(u => u.Player == player);
                var buildings = level.Buildings.Where(b => b.Player == player);
                var map = new ScalarField();
                foreach (var position in level.tiles.Keys)
                    map[position] = 0;
                foreach (var unit in units) {
                    float unitWorth = unit.type switch {
                        UnitType.Infantry => 1,
                        UnitType.AntiTank => 1.25f,
                        UnitType.LightTank => 3f,
                        UnitType.Artillery => 3f,
                        UnitType.MediumTank => 5f,
                        UnitType.Rockets => 5f,
                        UnitType.Apc => 1,
                        UnitType.Recon => 3f,
                        _ => 1
                    };
                    var influenceRange = 25;
                    foreach (var position in level.PositionsInRange(unit.NonNullPosition, new Vector2Int(0, influenceRange))) {
                        var distance = (position - unit.NonNullPosition).ManhattanLength();
                        //float influence = unitWorth * Mathf.Clamp01((float)(influenceRange - distance) / influenceRange);
                        float influence = unitWorth * (1 - (float)distance / influenceRange);
                        map[position] += influence;
                    }
                }
                return map;
            }

            var redMap = UnitInfluence(ColorName.Red);
            var blueMap = UnitInfluence(ColorName.Blue);

            var blueMapNormalized = new ScalarField(blueMap);
            TryNormalize(blueMapNormalized);

            var tension = redMap.Keys.ToDictionary(position => position, position => (blueMap[position] * redMap[position]));
            var presence = redMap.Keys.ToDictionary(position => position, position => (blueMap[position] - redMap[position]));
            {
                var gradient = GradientMap(presence);
                var flow = gradient.Keys.ToDictionary(position => position, position => gradient[position] * tension[position]);
                DrawVectorField(flow);
            }

            //DrawScalarField(map, true);
            return;
        }
    }

    public static void DrawVectorField(VectorField vectorField) {
        if (vectorField.Count == 0)
            return;
        var min = 0f;
        var max = vectorField.Values.Max(vector => vector.magnitude);
        using (Draw.ingame.WithLineWidth(1f))
            foreach (var (position, vector) in vectorField) {
                var position3D = position.Raycasted();
                var vector3D = vector.ToVector3();
                var t = Mathf.InverseLerp(min, max, vector.magnitude);
                var hue = Mathf.Lerp(.5f, 0, t);
                var color = Color.HSVToRGB(hue, 1, 1);
                Draw.ingame.Arrow(position3D - vector3D.normalized * t * .5f, position3D + vector3D.normalized * t * .5f, new Color(0, 0, 0, .75f));
                position3D += Vector3.up * .025f;
                Draw.ingame.Arrow(position3D - vector3D.normalized * t * .5f, position3D + vector3D.normalized * t * .5f, color);
            }
    }
    public static void DrawScalarField(ScalarField scalarField, bool symmetric = false) {
        if (scalarField.Count == 0)
            return;
        var min = scalarField.Values.Min();
        var max = scalarField.Values.Max();
        if (symmetric) {
            var range = Mathf.Max(Mathf.Abs(min), Mathf.Abs(max));
            (min, max) = (-range, range);
        }
        foreach (var (position, value) in scalarField) {
            var position3D = position.Raycasted();
            var t = Mathf.InverseLerp(min, max, value);
            var hue = Mathf.Lerp(.5f, 0, t);
            var color = Color.HSVToRGB(hue, 1, 1) * new Color(1, 1, 1, .5f);
            //color = new Color(1,0,0,t);
            Draw.ingame.SolidCircleXZ(position3D, .125f, new Color(0, 0, 0, .5f));
            position3D += Vector3.up * .025f;
            Draw.ingame.SolidCircleXZ(position3D, .125f, color);
            Draw.ingame.Label3D(position3D + new Vector3(0, 0, -.33f), Quaternion.Euler(90, 0, 0), $"{value:0.00}", .2f, LabelAlignment.Center, new Color(.5f, 1, 0, .25f));
        }
    }

    public static Dictionary<Vector2Int, Vector2> GradientMap(ScalarField map) {
        var gradientMap = new Dictionary<Vector2Int, Vector2>();
        foreach (var position in map.Keys) {
            var current = map[position];
            var right = map.TryGetValue(position + Vector2Int.right, out var value) ? value : current;
            var up = map.TryGetValue(position + Vector2Int.up, out value) ? value : current;
            var left = map.TryGetValue(position + Vector2Int.left, out value) ? value : current;
            var down = map.TryGetValue(position + Vector2Int.down, out value) ? value : current;
            var gradientX = (right - left) / 2;
            var gradientY = (up - down) / 2;
            gradientMap[position] = new Vector2(gradientX, gradientY);
        }
        return gradientMap;
    }

    private static List<Vector2Int> keys = new();

    public static bool TryNormalize(ScalarField map) {
        var low = map.Values.Min();
        var high = map.Values.Max();
        if (Mathf.Approximately(low - high, 0))
            return false;
        keys.Clear();
        keys.AddRange(map.Keys);
        foreach (var position in keys) {
            var value = map[position];
            var t = Mathf.InverseLerp(low, high, value);
            map[position] = t;
        }
        return true;
    }
}