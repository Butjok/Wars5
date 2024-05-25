using System;
using System.Collections;
using System.Linq;
using Drawing;
using Unity.Mathematics;
using UnityEngine;
using InfluenceMap = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, float>;

public class InfluenceMapDrawer : MonoBehaviour {

    public Vector2Int subdivisions = new(1, 1);
    public Vector2 hueRange = new(.5f, 0);
    public float noiseScale = 1;
    public Color fade = new(1, 1, 1, .25f);
    public IEnumerator coroutine;
    public bool draw = true;

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Backspace))
            draw = !draw;

        if (draw && Game.Instance) {
            var level = Game.Instance.Level;

            const int range = 25;

            InfluenceMap UnitInfluence(ColorName colorName) {
                var player = level.players.Single(p => p.ColorName == colorName);
                var units = level.Units.Where(u => u.Player == player);
                var buildings = level.Buildings.Where(b => b.Player == player);
                var map = new InfluenceMap();
                foreach (var position in level.tiles.Keys)
                    map[position] = 0;
                foreach (var unit in units) {
                    float unitWorth = unit.type switch {
                        UnitType.Infantry => 1,
                        UnitType.AntiTank => 1.25f,
                        UnitType.LightTank => 1.5f,
                        UnitType.Artillery => 1.5f,
                        UnitType.MediumTank => 3f,
                        UnitType.Rockets => 3f,
                        UnitType.Apc => 1,
                        UnitType.Recon => 1.25f,
                        _ => 1
                    };
                    foreach (var position in level.PositionsInRange(unit.NonNullPosition, new Vector2Int(1, range - 1))) {
                        var distance = (position - unit.NonNullPosition).ManhattanLength();
                        float influence = unitWorth * (range - distance);
                        map[position] += influence;
                        //map[position] = Mathf.Max(map[position], influence);
                    }
                }
                return map;
            }

            var redUnitInfluence = UnitInfluence(ColorName.Red);
            var blueUnitInfluence = UnitInfluence(ColorName.Blue);

            var influenceGradient = new InfluenceMap();
            foreach (var position in level.tiles.Keys) {
                influenceGradient[position] = redUnitInfluence[position] * blueUnitInfluence[position];
            }

            var map = influenceGradient;

            if (map.Count == 0)
                return;
            var min = map.Values.Min();
            var max = map.Values.Max();
            //max = 15;
            var step = Vector2.one / subdivisions;
            foreach (var (position, value) in map) {
                var ran = Mathf.Max(Mathf.Abs(min),  Mathf.Abs(max));
                //(min,max) = (-ran, ran);
                var t = Mathf.InverseLerp(min, max, value);
                var hue = Mathf.Lerp(hueRange.x, hueRange.y, t);
                var color = Color.HSVToRGB(hue, 1, 1);
                Draw.ingame.Label3D((position).Raycasted(), Quaternion.Euler(90, 0, 0), $"{value:F2}", .2f, LabelAlignment.Center, Color.black);
                for (var y = 0; y < subdivisions.y; y++)
                for (var x = 0; x < subdivisions.x; x++) {
                    var offset = new Vector2(x + .5f, y + .5f) * step;
                    Draw.ingame.SolidPlane((position - Vector2.one / 2 + offset).Raycasted(), quaternion.identity, Vector2.one / subdivisions, color * fade);
                }
            }
        }
    }
}