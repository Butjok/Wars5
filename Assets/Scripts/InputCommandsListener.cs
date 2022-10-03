using System;
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
        game.input.selectionPosition = new Vector2Int(x, y);
    }

    [Command]
    public void ClearPath() {
        game.input.pathBuilder?.Clear();
    }

    [Command]
    public void AddPathPosition(int x, int y) {
        var position = new Vector2Int(x, y);
        game.input.pathBuilder?.Add(position);
    }

    [Command]
    public void ReconstructPath(int x, int y) {
        var targetPosition = new Vector2Int(x, y);
        var positions = game.input.traverser.ReconstructPath(targetPosition)?.Skip(1);
        if (positions == null)
            return;
        game.input.pathBuilder.Clear();
        foreach (var position in positions)
            game.input.pathBuilder.Add(position);
    }

    [Command]
    public void Move() {
        if (game.input.pathBuilder == null)
            return;
        game.input.path = new MovePath(game.input.pathBuilder.Positions, game.input.startForward);
    }

    private UnitAction NewUnitAction(UnitActionType type, Unit targetUnit = null, Building targetBuilding = null, int weaponIndex = -1, Vector2Int targetPosition = default) {
        return new UnitAction(type, game.input.Unit, game.input.path, targetUnit, targetBuilding, weaponIndex, targetPosition);
    }

    [Command]
    public void Stay() {
        Assert.IsTrue(Rules.CanStay(game.input.Unit,game.input.path.Destination));
        game.input.action=NewUnitAction(UnitActionType.Stay);
    }

    [Command]
    public void Capture() {
        var found = game.TryGetBuilding(game.input.path.Destination, out var building);
        Assert.IsTrue(found);
        Assert.IsTrue(Rules.CanCapture(game.input.Unit, building));
        game.input.action = NewUnitAction(UnitActionType.Capture, targetBuilding: building);
    }

    [Command]
    public void Join() {
        var found = game.TryGetUnit(game.input.path.Destination, out var other);
        Assert.IsTrue(found);
        Assert.IsTrue(Rules.CanJoin(game.input.Unit, other));
        game.input.action = NewUnitAction(UnitActionType.Join, other);
    }

    [Command]
    public void GetIn() {
        var found = game.TryGetUnit(game.input.path.Destination, out var other);
        Assert.IsTrue(found);
        Assert.IsTrue(Rules.CanLoadAsCargo(other,game.input.Unit));
        game.input.action = NewUnitAction(UnitActionType.GetIn, other);
    }

    [Command]
    public void Supply(int x, int y) {
        var position = new Vector2Int(x, y);
        var found = game.TryGetUnit(position, out var other);
        Assert.IsTrue(found);
        Assert.IsTrue(Rules.CanSupply(game.input.Unit, other));
        game.input.action = NewUnitAction(UnitActionType.Supply, other);
    }

    [Command]
    public void Drop(int index, int x, int y) {
        Assert.IsTrue(index >= 0);
        Assert.IsTrue(index < game.input.Unit.cargo.Count);
        var cargo = game.input.Unit.cargo[index];
        var position = new Vector2Int(x, y);
        Assert.IsTrue(Rules.CanStay(cargo, position));
        game.input.action = NewUnitAction(UnitActionType.DropOut, cargo, targetPosition: position);
    }

    [Command]
    public void Attack(int x, int y, int weaponIndex = 0) {
        var position = new Vector2Int(x, y);
        var found = game.TryGetUnit(position, out var target);
        Assert.IsTrue(found);
        Assert.IsTrue(Rules.CanAttack(game.input.Unit, target, game.input.path, weaponIndex));
        game.input.action = NewUnitAction(UnitActionType.Attack, target, weaponIndex:weaponIndex);
    }

    [Command]
    public void Build(string unitTypeName) {
        var parsed = Enum.TryParse(unitTypeName, out UnitType unitType);
        Assert.IsTrue(parsed);
        game.input.unitType = unitType;
    }

    [Command]
    public void Break() { }
}

[Serializable]
public class InputCommands {

    public Game2 game;

    public Vector2Int? selectionPosition;
    
    private Unit unit;
    public Unit Unit {
        get => unit;
        set {
            unit = value;

            if (Unit == null) {
                pathBuilder = null;
                return;
            }

            Assert.IsTrue(Unit.position.v != null);
            var unitPosition = (Vector2Int)Unit.position.v;

            startForward = unit.view.transform.forward.ToVector2().RoundToInt();
            pathBuilder = new MovePathBuilder(unitPosition);
            path = null;
            traverser.Traverse(game.tiles.Keys, unitPosition, (position, length) => Cost(Unit, position, length));
        }
    }

    public bool endTurn;
    public Vector2Int startForward;
    public MovePathBuilder pathBuilder;
    public MovePath path;
    public Building building;
    public UnitType unitType;
    public UnitAction action;
    public Traverser traverser = new();

    public InputCommands(Game2 game) {
        this.game = game;
    }

    public int? Cost(Unit unit, Vector2Int position, int length) {
        if (length >= Rules.MoveDistance(unit) ||
            !game.TryGetTile(position, out var tile) ||
            game.TryGetUnit(position, out var other) && !Rules.CanPass(unit, other))
            return null;

        return Rules.MoveCost(unit, tile);
    }

    public void Reset() {
        selectionPosition = null;
        unit = null;
        endTurn = false;
        startForward = Vector2Int.zero;
        pathBuilder = null;
        path = null;
        building = null;
        unitType = 0;
        action = null;
    }
}