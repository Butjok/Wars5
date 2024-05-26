using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using static PathFinder;

public class ScalarField2 {
    private readonly HashSet<Vector2Int> domain;
    private readonly Func<Vector2Int, float> function;
    public ScalarField2(IEnumerable<Vector2Int> domain, Func<Vector2Int, float> function) {
        this.domain = domain.ToHashSet();
        this.function = function;
    }
    public float this[Vector2Int position] {
        get {
            Assert.IsTrue(domain.Contains(position));
            return function(position);
        }
    }
    public IReadOnlyCollection<Vector2Int> Domain => domain;
}

public static class OcclusionField {
    public static ScalarField2 Calculate(Dictionary<Vector2Int, TileType> tiles, ScalarField2 radiusField2, Func<Vector2Int, bool> isObstacle) {
        var values = new Dictionary<Vector2Int, int>();
        foreach (var seed in tiles.Keys)
            if (isObstacle(seed))
                values[seed] = 0;
            else {
                var radius = Mathf.RoundToInt(radiusField2[seed]);
                var area = seed.GrownBy(radius).Intersect(tiles.Keys).ToHashSet();
                var closed = new HashSet<Vector2Int> { seed };
                var queue = new Queue<Vector2Int>();
                queue.Enqueue(seed);
                while (queue.TryDequeue(out var position))
                    foreach (var nearPosition in position.GrownBy(1).Intersect(area).Except(closed).Where(isObstacle.Negated())) {
                        queue.Enqueue(nearPosition);
                        closed.Add(nearPosition);
                    }
                values[seed] = closed.Count;
            }
        return new ScalarField2(values.Keys, position => values[position]);
    }
}

public static class ScalarFieldCalculator {

    public static ScalarField2 Evaluate(string input, Game game, int level = 0, Stack stack = null, Unit ignoredUnit = null) {

        stack ??= new Stack();

        var tokens = Tokenizer.Tokenize(input).Select(token => token.ToString());
        foreach (var token in tokens) {

            var tiles = game.TryGetLevel.tiles;

            switch (token) {

                case "norm": {
                    var field = (ScalarField2)stack.Pop();
                    Assert.IsTrue(field.Domain.Count > 0);
                    var min = field.Domain.Min(position => field[position]);
                    var max = field.Domain.Max(position => field[position]);
                    stack.Push(new ScalarField2(field.Domain, position => (field[position] - min) / (max - min)));
                    break;
                }

                case "gradient": {
                    var field = (ScalarField2)stack.Pop();
                    stack.Push(new ScalarField2(field.Domain, p => {
                        float A(int nx, int ny) {
                            var n = new Vector2Int(nx, ny);
                            return field.Domain.Contains(n) ? field[n] : A(p.x, p.y);
                        }
                        var dx = A(p.x + 1, p.y) - A(p.x - 1, p.y);
                        var dy = A(p.x, p.y + 1) - A(p.x, p.y - 1);
                        return (Mathf.Abs(dx) + Mathf.Abs(dy)) / 2;
                    }));
                    break;
                }

                case "infl": {
                    var colorName = Enum.Parse<ColorName>((string)stack.Pop());
                    var player = game.TryGetLevel.players.Single(p => p.ColorName == colorName);
                    var units = game.TryGetLevel.units.Values.Where(u => u.Player == player && u != ignoredUnit);
                    var influences = game.TryGetLevel.tiles.Keys.ToDictionary(
                        position => position,
                        position => units.Sum(unit => UnitStats.Loaded.TryGetValue(unit.type, out var stats) && game.TryGetLevel.precalculatedDistances != null && game.TryGetLevel.precalculatedDistances.TryGetValue((stats.moveType, unit.NonNullPosition, position), out var distance)
                            ? Mathf.Max(0, Rules.MoveCapacity(unit) + 1 - distance)
                            : 0));
                    stack.Push(new ScalarField2(influences.Keys, position => influences[position]));
                    break;
                }

                case "pdist": {
                    var moveType = Enum.Parse<MoveType>((string)stack.Pop());
                    var colorName = Enum.Parse<ColorName>((string)stack.Pop());
                    var player = game.TryGetLevel.players.Single(p => p.ColorName == colorName);
                    var units = game.TryGetLevel.units.Values.Where(u => u.Player == player && u != ignoredUnit);
                    var distances = game.TryGetLevel.tiles.Keys.ToDictionary(
                        position => position,
                        position => units.Aggregate(infinity, (current, unit) => game.TryGetLevel.precalculatedDistances.TryGetValue((moveType, position, unit.NonNullPosition), out var distance)
                            ? Mathf.Min(current, distance)
                            : current));
                    stack.Push(new ScalarField2(distances.Keys.Where(position => distances[position] != infinity), position => distances[position]));
                    break;
                }

                case "round" or "abs" or "sqrt": {
                    var field = (ScalarField2)stack.Pop();
                    stack.Push(new ScalarField2(field.Domain, token switch {
                        "round" => position => Mathf.Round(field[position]),
                        "abs" => position => Mathf.Abs(field[position]),
                        "sqrt" => position => Mathf.Sqrt(field[position]),
                        _ => throw new ArgumentOutOfRangeException()
                    }));
                    break;
                }

                case "+" or "-" or "*" or "/" or "min" or "max": {
                    var b = (ScalarField2)stack.Pop();
                    var a = (ScalarField2)stack.Pop();
                    stack.Push(new ScalarField2(a.Domain.Intersect(b.Domain), token switch {
                        "+" => position => a[position] + b[position],
                        "-" => position => a[position] - b[position],
                        "*" => position => a[position] * b[position],
                        "/" => position => a[position] / b[position],
                        "min" => position => Mathf.Min(a[position], b[position]),
                        "max" => position => Mathf.Max(a[position], b[position]),
                        _ => throw new ArgumentOutOfRangeException()
                    }));
                    break;
                }

                case "smoothstep": {

                    float SmoothStep(float t) {
                        t = Mathf.Clamp01(t);
                        return t * t * (3.0f - 2.0f * t);
                    }

                    var max = (dynamic)stack.Pop();
                    var min = (dynamic)stack.Pop();
                    var field = (ScalarField2)stack.Pop();

                    stack.Push(new ScalarField2(field.Domain, position => {
                        var t = (field[position] - min[position]) / (max[position] - min[position]);
                        return SmoothStep(t);
                    }));
                    break;
                }

                case "dist": {
                    var moveType = (MoveType)stack.Pop();
                    // stack.Push(DistanceField2.Calculate(tiles, moveType));
                    break;
                }

                case "occl": {
                    var radiusField = (ScalarField2)stack.Pop();
                    stack.Push(OcclusionField.Calculate(tiles, radiusField, position => tiles[position] is TileType.Sea or TileType.Mountain));
                    break;
                }

                case "dup":
                    stack.Push(stack.Peek());
                    break;

                case "pop":
                    stack.Pop();
                    break;

                default:
                    if (int.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var integer)) {
                        var result = integer;
                        stack.Push(new ScalarField2(tiles.Keys, _ => result));
                    }
                    else if (float.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var real)) {
                        var result = real;
                        stack.Push(new ScalarField2(tiles.Keys, _ => result));
                    }
                    else
                        stack.ExecuteToken(token);
                    break;
            }
        }

        if (level == 0) {
            Assert.IsTrue(stack.Count == 1);
            return (ScalarField2)stack.Pop();
        }
        return null;
    }
}