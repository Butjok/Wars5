using System;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class InputCommandsListener : MonoBehaviour {

    public string inputPath = "";
    public string outputPath = "";

    public DateTime? lastWriteTime;
    public Game game;

    public bool executeOnStart = true;
    public List<string> history = new();

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        game = GetComponent<Game>();
        Assert.IsTrue(game);
    }
    
    private void Awake() {
        EnsureInitialized();
    }

    private void Start() {
        if (!executeOnStart)
            lastWriteTime = File.GetLastWriteTime(inputPath);
    }

    private void Update() {

        var lastWriteTime = File.GetLastWriteTime(inputPath);
        if (lastWriteTime != this.lastWriteTime) {
            this.lastWriteTime = lastWriteTime;
            Execute(File.ReadAllText(inputPath));
        }

        // if (Input.GetKeyDown(KeyCode.F5))
            // Write();
    }

    [Command]
    private void Execute(string input) {

        EnsureInitialized();
        
        if (string.IsNullOrWhiteSpace(input))
            return;
        
        history.Add(input);
                
        PostfixInterpreter.Execute(input, (command, stack) => {

            switch (command) {

                case "endTurn":
                    game.input.endTurn = true;
                    return true;

                case "select":
                    game.input.selectAt = stack.Pop<Vector2Int>();
                    return true;

                case "appendToPath":
                    game.input.appendToPath.Enqueue(stack.Pop<Vector2Int>());
                    return true;

                case "reconstructPath":
                    game.input.reconstructPathTo = stack.Pop<Vector2Int>();
                    return true;

                case "move":
                    game.input.moveUnit = true;
                    return true;

                case "stay":
                    game.input.actionFilter = action => action.type == UnitActionType.Stay;
                    return true;

                case "capture":
                    game.input.actionFilter = action => action.type == UnitActionType.Capture;
                    return true;

                case "join":
                    game.input.actionFilter = action => action.type == UnitActionType.Join;
                    return true;

                case "getIn":
                    game.input.actionFilter = action => action.type == UnitActionType.GetIn;
                    return true;

                case "supply":
                    game.input.actionFilter = action => action.type == UnitActionType.Supply &&
                                                        action.targetPosition == stack.Pop<Vector2Int>();
                    return true;

                case "drop": {

                    var position = stack.Pop<Vector2Int>();
                    var index = stack.Pop<int>();

                    game.input.actionFilter = action => action.type == UnitActionType.Drop &&
                                                        index < action.unit.cargo.Count &&
                                                        action.targetUnit == action.unit.cargo[index] &&
                                                        action.targetPosition == position;
                    return true;
                }

                case "attack": {

                    var weaponIndex = stack.Pop<int>();
                    var position = stack.Pop<Vector2Int>();

                    game.input.actionFilter = action => action.type == UnitActionType.Attack &&
                                                        action.targetUnit.position.v == position &&
                                                        action.weaponIndex == weaponIndex;
                    return true;
                }

                case "build":
                    game.input.buildUnitType = stack.Pop<UnitType>();
                    return true;
                        
                default:
                    return false;
            }
        });
    }
    //
    // [Command]
    // private void Write() {
    //     EnsureInitialized();
    //     File.WriteAllText(outputPath, new SerializedGame(game).ToJson());
    //     Debug.Log($"Written to: {outputPath}");
    // }
}

[Serializable]
public class InputCommandsContext {

    public Vector2Int? selectAt;
    public bool cancel;
    public bool endTurn;
    public UnitType buildUnitType;
    public Queue<Vector2Int> appendToPath = new();
    public Vector2Int? reconstructPathTo;
    public bool moveUnit;

    public Predicate<UnitAction> actionFilter;

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