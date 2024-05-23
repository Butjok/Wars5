using System;
using System.Collections;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Unity.Mathematics;
using UnityEngine;
using InfluenceMap = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, float>;

public class InfluenceMapDrawer : MonoBehaviour {

    public Vector2Int subdivisions = new(1, 1);
    public Vector2 hueRange = new(.5f, 0);
    public float noiseScale = 1;

    [Command]
    public void DrawInfluence() {
        StopAllCoroutines();
        StartCoroutine(DrawingCoroutine(() => {
            var level = Game.Instance.Level;

            InfluenceMap PlayerInfluence(ColorName colorName) {
                var player = level.players.Single(p => p.ColorName == colorName);
                var units = level.Units.Where(u => u.Player == player);
                var map = new InfluenceMap();
                foreach (var position in level.tiles.Keys)
                    map[position] = 0;
                foreach (var unit in units) {
                    float unitWorth = unit.type switch {
                        UnitType.Infantry => 1,
                        UnitType.AntiTank => 2,
                        UnitType.LightTank => 3,
                        UnitType.Artillery => 3,
                        UnitType.MediumTank => 5,
                        UnitType.Rockets => 5,
                        UnitType.Apc => 1,
                        UnitType.Recon => 2,
                        _ => 1
                    };
                    unitWorth = 1;
                    foreach (var position in level.PositionsInRange(unit.NonNullPosition, new Vector2Int(0, 5))) {
                        var distance = (position - unit.NonNullPosition).ManhattanLength();
                        var influence = 6-distance;
                        map[position] = Mathf.Max(map[position], influence);
                    }
                }
                return map;
            }

            var redInfluence = PlayerInfluence(ColorName.Red);
            var blueInfluence = PlayerInfluence(ColorName.Blue);

            var influence = new InfluenceMap();
            foreach (var position in level.tiles.Keys) {
                var red = redInfluence[position];
                var blue = blueInfluence[position];
                influence[position] = red - blue;
            }

            var influenceGradient = new InfluenceMap();
            foreach (var position in level.tiles.Keys) {
                influenceGradient[position] = 0;
                if (influence.TryGetValue(position + Vector2Int.right, out var right) &&
                    influence.TryGetValue(position + Vector2Int.left, out var left) &&
                    influence.TryGetValue(position + Vector2Int.up, out var up) &&
                    influence.TryGetValue(position + Vector2Int.down, out var down)) {
                    var dx = right - left;
                    var dy = up - down;
                    influenceGradient[position] = Mathf.Abs(dx) + Mathf.Abs(dy);
                }
            }

            return influence;
        }));
    }

    public IEnumerator DrawingCoroutine(Func<InfluenceMap> influenceMapGetter) {
        while (!Input.GetKeyDown(KeyCode.Backspace)) {
            yield return null;
            var map = influenceMapGetter();
            if (map.Count == 0)
                continue;
            var min = map.Values.Min();
            var max = map.Values.Max();
            var step = Vector2.one / subdivisions;
            foreach (var (position, value) in map) {
                var t = Mathf.InverseLerp(min, max, value);
                var hue = Mathf.Lerp(hueRange.x, hueRange.y, t);
                var color = Color.HSVToRGB(hue, 1, 1);
                Draw.ingame.Label3D((position).Raycasted(), Quaternion.Euler(90, 0, 0), $"{value:F2}", .2f, LabelAlignment.Center, Color.black);
                for (var y = 0; y < subdivisions.y; y++)
                for (var x = 0; x < subdivisions.x; x++) {
                    var offset = new Vector2(x + .5f, y + .5f) * step;
                    Draw.ingame.SolidPlane((position - Vector2.one / 2 + offset).Raycasted(), quaternion.identity, Vector2.one / subdivisions, color);
                }
            }
        }
    }
}