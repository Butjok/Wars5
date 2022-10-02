using System;
using System.IO;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class CommandListener : MonoBehaviour {

    public string inputPath = "";
    public string outputPath = "";
    public Game2 game;
    public bool executeOnStart = false;

    public DateTime lastWriteTime;

    public void Start() {
        Assert.IsTrue(game);
        lastWriteTime = File.GetLastWriteTime(inputPath);
        if (executeOnStart)
            Execute();
    }

    public void Execute() {
        var input = File.ReadAllText(inputPath);
        Interpreter.Execute(input);
    }

    public void Update() {
        var lastWriteTime =  File.GetLastWriteTime(inputPath);
        if (lastWriteTime != this.lastWriteTime) {
            this.lastWriteTime = lastWriteTime;
            Execute();
        }
    }

    [Command]
    public void Write() {
        File.WriteAllText(outputPath, new SerializedGame(game).ToJson());
    }
    
    [Command]
    private void Move(Vector2Int from, Vector2Int to) {
        Assert.IsTrue(game.TryGetUnit(from,out var unit));
        unit.position.v = to;
        unit.moved.v = true;
    }
    
    [Command]
    public void Attack(Vector2Int from, Vector2Int to, Vector2Int target) {
        Move(from, to);
        // TODO: attack
    }
    
    [Command]
    public void Supply(Vector2Int from, Vector2Int to, Vector2Int target) {
        Move(from, to);
        // TODO: attack
    }
    
    [Command]
    public void Build(Vector2Int position, string typeName) {
        Assert.IsTrue(game.TryGetBuilding(position, out var building));
        Assert.IsTrue(building.player.v!=null);
        Assert.IsTrue(!game.TryGetUnit(position,out _));
        var type = Enum.Parse<UnitType>(typeName);
        Assert.IsTrue((Rules.BuildableUnits(building.type) & type) != 0);
        game.units[building.position] = new Unit(building.player.v, type: type, position: position);
    }

    [Command]
    public void EndTurn() {
        
    }
}