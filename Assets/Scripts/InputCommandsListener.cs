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
    public Level level;

    public bool executeOnStart = true;
    public List<string> history = new();

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;
        level = GetComponent<Level>();
        Assert.IsTrue(level);
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
                    level.input.endTurn = true;
                    return true;

                case "select":
                    level.input.selectAt = stack.Pop<Vector2Int>();
                    return true;

                case "appendToPath":
                    level.input.appendToPath.Enqueue(stack.Pop<Vector2Int>());
                    return true;

                case "reconstructPath":
                    level.input.reconstructPathTo = stack.Pop<Vector2Int>();
                    return true;

                case "move":
                    level.input.moveUnit = true;
                    return true;

                case "stay":
                    level.input.actionFilter = action => action.type == UnitActionType.Stay;
                    return true;

                case "capture":
                    level.input.actionFilter = action => action.type == UnitActionType.Capture;
                    return true;

                case "join":
                    level.input.actionFilter = action => action.type == UnitActionType.Join;
                    return true;

                case "getIn":
                    level.input.actionFilter = action => action.type == UnitActionType.GetIn;
                    return true;

                case "supply":
                    level.input.actionFilter = action => action.type == UnitActionType.Supply &&
                                                        action.targetPosition == stack.Pop<Vector2Int>();
                    return true;

                case "drop": {

                    var position = stack.Pop<Vector2Int>();
                    var index = stack.Pop<int>();

                    level.input.actionFilter = action => action.type == UnitActionType.Drop &&
                                                        index < action.unit.cargo.Count &&
                                                        action.targetUnit == action.unit.cargo[index] &&
                                                        action.targetPosition == position;
                    return true;
                }

                case "attack": {

                    var weaponIndex = stack.Pop<int>();
                    var position = stack.Pop<Vector2Int>();

                    level.input.actionFilter = action => action.type == UnitActionType.Attack &&
                                                        action.targetUnit.position.v == position &&
                                                        action.weaponIndex == weaponIndex;
                    return true;
                }

                case "build":
                    level.input.buildUnitType = stack.Pop<UnitType>();
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