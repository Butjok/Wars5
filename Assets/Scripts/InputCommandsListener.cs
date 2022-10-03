using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class InputCommandsListener : MonoBehaviour {

    public string inputPath = "";
    public string outputPath = "";

    public DateTime? lastWriteTime;
    public Game2 game;

    public void Awake() {
        game = GetComponent<Game2>();
        Assert.IsTrue(game);
    }

    public void Update() {
        var lastWriteTime = File.GetLastWriteTime(inputPath);
        if (lastWriteTime != this.lastWriteTime) {
            this.lastWriteTime = lastWriteTime;
            Interpreter.Execute(File.ReadAllText(inputPath));
        }
    }

    [Command]
    public void Write() {
        File.WriteAllText(outputPath, new SerializedGame(game).ToJson());
    }

    [Command]
    public void EndTurn() {
        game.input.endTurn = true;
    }

    [Command]
    public void Select(int x, int y) {
        game.input.selectAt = new Vector2Int(x, y);
    }

    [Command]
    public void AppendToPath(int x, int y) {
        game.input.appendToPath.Enqueue(new Vector2Int(x, y));
    }

    [Command]
    public void ReconstructPath(int x, int y) {
        game.input.reconstructPathTo = new Vector2Int(x, y);
    }

    [Command]
    public void Move() {
        game.input.moveUnit = true;
    }

    [Command]
    public void Stay() {
        game.input.actionFilter = action => action.type == UnitActionType.Stay;
    }

    [Command]
    public void Capture() {
        game.input.actionFilter = action => action.type == UnitActionType.Capture;
    }

    [Command]
    public void Join() {
        game.input.actionFilter = action => action.type == UnitActionType.Join;
    }

    [Command]
    public void GetIn() {
        game.input.actionFilter = action => action.type == UnitActionType.GetIn;
    }

    [Command]
    public void Supply(int x, int y) {
        game.input.actionFilter = action => action.type == UnitActionType.Supply && 
                                            action.targetPosition == new Vector2Int(x, y);
    }

    [Command]
    public void Drop(int index, int x, int y) {
        game.input.actionFilter = action => action.type == UnitActionType.Drop &&
                                            index < action.unit.cargo.Count &&
                                            action.targetUnit == action.unit.cargo[index] &&
                                            action.targetPosition == new Vector2Int(x, y);
    }

    [Command]
    public void Attack(int x, int y, int weaponIndex = 0) {
        game.input.actionFilter = action => action.type == UnitActionType.Attack &&
                                            action.targetUnit.position.v == new Vector2Int(x, y) &&
                                            action.weaponIndex == weaponIndex;
    }

    [Command]
    public void Build(string unitTypeName) {
        var parsed = Enum.TryParse(unitTypeName, out UnitType unitType);
        Assert.IsTrue(parsed);
        game.input.buildUnitType = unitType;
    }

    [Command]
    public void Break() { }
}

[Serializable]
public class InputCommands {

    public Game2 game;

    public Vector2Int? selectAt;
    public bool cancel;
    public bool endTurn;
    public UnitType buildUnitType;
    public Queue<Vector2Int> appendToPath = new();
    public Vector2Int? reconstructPathTo;
    public bool moveUnit;

    public Predicate<UnitAction> actionFilter;

    public InputCommands(Game2 game) {
        this.game = game;
    }

    public void Reset() {
        selectAt = null;
        cancel = false;
        endTurn = false;
        buildUnitType = 0;
        appendToPath.Clear();
        reconstructPathTo = null;
        moveUnit = false;
        actionFilter = null;
    }
}