using System;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class CommandsListener : MonoBehaviour {

    public string inputPath = "";
    public string outputPath = "";
    
    public DateTime? lastWriteTime;
    
    public Game2 game;
    private CommandsContext Context => game.commandsContext;

    public void Awake() {
        game = GetComponent<Game2>();
        Assert.IsTrue(game);
    }

    public void Update() {
        var lastWriteTime = File.GetLastWriteTime(inputPath);
        if (lastWriteTime != this.lastWriteTime) {
            this.lastWriteTime = lastWriteTime;
            
            var input = File.ReadAllText(inputPath);
            Interpreter.Execute(input);
        }
    }

    [Command]
    public void Write() {
        File.WriteAllText(outputPath, new SerializedGame(game).ToJson());
    }
    
    [Command]
    public void EndTurn() {
        Context.endTurn = true;
    }

    [Command]
    public void SelectUnit(Vector2Int position) {
        var found = game.TryGetUnit(position, out Context.unit);
        Assert.IsTrue(found);
    }

    [Command]
    public void ClearPath() {
        Context.pathPositions.Clear();
    }

    [Command]
    public void AddPathPosition(Vector2Int position) {
        Context.pathPositions.Add(position);
    }

    [Command]
    public void SelectBuilding(Vector2Int position) {
        var found = game.TryGetBuilding(position, out Context.building);
        Assert.IsTrue(found);
    }

    [Command]
    public void Build(UnitType unitType) {
        Assert.IsTrue(Context.building != null);
        Assert.IsTrue(Context.building.player.v != null);
        Assert.IsTrue(!game.TryGetUnit(Context.building.position, out _));
        new Unit(Context.building.player.v, type: unitType);
    }
}

public class CommandsContext {
    
    public bool endTurn;
    public Unit unit;
    public List<Vector2Int> pathPositions = new();
    public bool move;
    public Building building;

    public void Clear() {
        endTurn = false;
        unit = null;
        pathPositions.Clear();
        building = null;
        move = false;
    }
}