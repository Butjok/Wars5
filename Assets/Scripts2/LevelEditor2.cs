using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditor2 {

    public const string prefix = "level-editor.";

    public const string selectTilesMode = prefix + "select-tiles-mode";
    public const string selectUnitsMode = prefix + "select-units-mode";

    public const string cycleTileType = prefix + "cycle-tile-type";
    public const string placeTile = prefix + "place-tile";
    public const string removeTile = prefix + "remove-tile";

    public const string cycleUnitType = prefix + "cycle-unit";
    public const string placeUnit = prefix + "place-unit";
    public const string removeUnit = prefix + "remove-unit";
    public const string cycleLookDirection = prefix + "cycle-look-direction";

    public const string cyclePlayer = prefix + "cycle-players";
    public const string play = prefix + "play";

    public const string mode = nameof(mode);

    public Main main;
    public LevelEditorTextDisplay textDisplay;

    public LevelEditor2(Main main, TMP_Text text) {
        this.main = main;
        textDisplay = new LevelEditorTextDisplay(text);
        textDisplay.Clear();
    }

    public Vector2Int lookDirection = Vector2Int.up;
    public Vector2Int[] lookDirections = Rules.offsets;

    public IEnumerator Run() {

        main.Clear();

        var red = new Player(main, Color.red, Team.Alpha);
        var blue = new Player(main, Color.blue, Team.Bravo);
        player = red;

        var min = new Vector2Int(-5, -5);
        var max = new Vector2Int(5, 5);

        for (var y = min.y; y <= min.y; y++)
        for (var x = min.x; x <= min.x; x++)
            main.tiles.Add(new Vector2Int(x, y), TileType.Plain);

        new Building(main, min, TileType.Hq, red);
        new Building(main, max, TileType.Hq, blue);

        new Unit(red, UnitType.Infantry, min);
        new Unit(blue, UnitType.Infantry, max);

        yield return TilesMode();
    }

    public TileType tileType = TileType.Plain;
    public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Sea, TileType.Mountain, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard };

    public Player player;

    public IEnumerator TilesMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(TilesMode));
        textDisplay.Set(nameof(tileType), tileType);
        textDisplay.Set(nameof(player), player);

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F8))
                main.commands.Enqueue(selectUnitsMode);
            else if (Input.GetKeyDown(KeyCode.Tab)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cycleTileType);
            }
            else if (Input.GetKeyDown(KeyCode.F2)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cyclePlayer);
            }
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position)) {
                main.stack.Push(position);
                main.commands.Enqueue(placeTile);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int position2)) {
                main.stack.Push(position2);
                main.commands.Enqueue(removeTile);
            }

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectUnitsMode:
                            yield return UnitsMode();
                            yield break;

                        case cycleTileType:
                            tileType = CycleValue(tileType, tileTypes, main.stack.Pop<int>());
                            textDisplay.Set(nameof(tileType), tileType);
                            break;
                        
                        case cyclePlayer:
                            player = CycleValue(player, main.players, main.stack.Pop<int>());
                            textDisplay.Set(nameof(player), player);
                            break;

                        case placeTile: {
                            break;
                        }

                        case removeTile: {
                            break;
                        }
                        
                        case play:
                            yield return Play();
                            yield return TilesMode();
                            yield break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }

    public UnitType unitType = UnitType.Infantry;
    public UnitType[] unitTypes = { UnitType.Infantry, UnitType.AntiTank, UnitType.Artillery, UnitType.Apc, UnitType.TransportHelicopter, UnitType.AttackHelicopter, UnitType.FighterJet, UnitType.Bomber, UnitType.Recon, UnitType.LightTank, UnitType.Rockets, };

    public IEnumerator UnitsMode() {

        textDisplay.Clear();
        textDisplay.Set(mode, nameof(UnitsMode));
        textDisplay.Set(nameof(unitType), unitType);
        textDisplay.Set(nameof(lookDirection), lookDirection);
        textDisplay.Set(nameof(player), player);

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.F8))
                main.commands.Enqueue(selectTilesMode);
            
            else if (Input.GetKeyDown(KeyCode.Tab)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cycleUnitType);
            }
            else if (Input.GetKeyDown(KeyCode.F2)) {
                main.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
                main.commands.Enqueue(cyclePlayer);
            }
            else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position)) {
                main.stack.Push(position);
                main.commands.Enqueue(placeUnit);
            }
            else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int position2)) {
                main.stack.Push(position2);
                main.commands.Enqueue(removeUnit);
            }
            else if (Input.GetKeyDown(KeyCode.F5)) {
                
            }

            while (main.commands.TryDequeue(out var command))
                foreach (var token in command.Tokenize())
                    switch (token) {

                        case selectTilesMode:
                            yield return TilesMode();
                            yield break;

                        case cyclePlayer:
                            player = CycleValue(player, main.players, main.stack.Pop<int>());
                            textDisplay.Set(nameof(player), player);
                            break;

                        case cycleUnitType:
                            unitType = CycleValue(unitType, unitTypes, main.stack.Pop<int>());
                            textDisplay.Set(nameof(unitType), unitType);
                            break;
                        
                        case cycleLookDirection:
                            lookDirection = CycleValue(lookDirection, lookDirections, main.stack.Pop<int>());
                            textDisplay.Set(nameof(lookDirection), lookDirection);
                            break;

                        case play:
                            yield return Play();
                            yield return UnitsMode();
                            yield break;

                        case placeUnit: {
                            break;
                        }

                        case removeUnit: {
                            break;
                        }

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }

    public IEnumerator Play() {
        yield break;
    }

    public static T CycleValue<T>(T value, T[] values, int offset = 1) {
        var index = Array.IndexOf(values, value);
        Assert.AreNotEqual(-1, index);
        return values[(index + offset).PositiveModulo(values.Length)];
    }
    public static T CycleValue<T>(T value, List<T> values, int offset = 1) {
        var index = values.IndexOf(value);
        Assert.AreNotEqual(-1, index);
        return values[(index + offset).PositiveModulo(values.Count)];
    }
}