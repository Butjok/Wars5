using System.Collections.Generic;
using Butjok.CommandLine;
using Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditorPathsModeState : StateMachineState {

    [Command]
    public static float lineWidth = 2;
    [Command]
    public static float unitSpeed = 2;
    [Command]
    public static Color inactiveTint = new(1, 1, 1, .25f);
    [Command]
    public static Color activeTint = Color.white * 1000;

    public LevelEditorGui gui;
    public Level level;
    public Level.Path selectedPath;
    public LinkedListNode<Vector2Int> selectedNode;

    public LevelEditorPathsModeState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Enter {
        get {
            Vector3 Raycast(Vector2Int position) {
                if (position.TryRaycast(out var hit))
                    return hit.point;
                return position.ToVector3();
            }

            var editorState = stateMachine.TryFind<LevelEditorSessionState>();
            gui = editorState.gui;
            level = editorState.level;
            var cameraRig = level.view.cameraRig;
            var camera = cameraRig.camera;

            gui.layerStack.Push(() => {
                GUILayout.Label("Level editor > Paths");
                GUILayout.Space(DefaultGuiSkin.defaultSpacingSize);
                GUILayout.Label($"Paths:");
                for (var i = 0; i < level.paths.Count; i++) {
                    var name = level.paths[i].name;
                    GUILayout.Label(level.paths[i] == selectedPath ? $"[{name}]" : $" {name}");
                }
            });

            void DrawPath(Level.Path path, Color tint) {
                var index = 0;
                using (Draw.ingame.WithLineWidth(lineWidth))
                    for (var node = path.list.First; node != null; node = node.Next) {
                        var position = Raycast(node.Value);
                        Draw.ingame.CircleXZ(position, 0.25f, Color.yellow * tint);
                        if (node == selectedNode)
                            Draw.ingame.CircleXZ(position, 0.33f, Color.green * tint);
                        if (node.Previous == null)
                            Draw.ingame.Label3D(position, quaternion.identity, '\n' + path.name, .25f, LabelAlignment.Center, Color.yellow * tint);
                        Draw.ingame.Label3D(position, quaternion.identity, index.ToString(), .25f, LabelAlignment.Center, Color.yellow * tint);
                        if (node.Previous != null)
                            Draw.ingame.Line(Raycast(node.Previous.Value), position, Color.yellow * tint);
                        index++;
                    }
            }

            void DrawPaths() {
                foreach (var path in level.paths)
                    DrawPath(path, path == selectedPath ? activeTint : inactiveTint);
            }

            bool TryGetMousePosition(out Vector2Int result) {
                if (camera.TryPhysicsRaycast(out Vector3 hitPoint) || camera.TryRaycastPlane(out hitPoint)) {
                    result = hitPoint.ToVector2Int();
                    return true;
                }

                result = default;
                return false;
            }

            while (true) {
                yield return StateChange.none;

                if (TryEnqueueModeSelectionCommand()) { }
                else if (Input.GetKeyDown(KeyCode.Tab)) {
                    if (level.paths.Count > 0) {
                        var index = level.paths.IndexOf(selectedPath);
                        var offset =  Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
                        var nextIndex = (index + offset + level.paths.Count) % level.paths.Count;
                        selectedPath = level.paths[nextIndex];
                        selectedNode = selectedPath.list.Last;
                    }
                }
                else if (selectedNode != null && Input.GetKeyDown(KeyCode.X)) {
                    if (selectedPath.list.Count > 1) {
                        selectedPath.list.Remove(selectedNode);
                        selectedNode = selectedNode.Previous;
                    }
                    else {
                        level.paths.Remove(selectedPath);
                        selectedPath = null;
                        selectedNode = null;
                    }
                }
                else if (TryGetMousePosition(out var mousePosition)) {
                    if (Input.GetMouseButtonDown(Mouse.left) && selectedPath != null) {
                        for (var node = selectedPath.list.First; node != null; node = node.Next)
                            if (node.Value == mousePosition) {
                                selectedNode = node;
                                break;
                            }
                    }
                    else if (Input.GetMouseButtonDown(Mouse.right) && selectedNode != null)
                        selectedNode = null;
                    else if (selectedNode != null && Input.GetKeyDown(KeyCode.G)) {
                        // move a node
                        if (!Input.GetKey(KeyCode.LeftShift)) {
                            var originalPosition = selectedNode.Value;
                            while (TryGetMousePosition(out mousePosition)) {
                                selectedNode.Value = mousePosition;
                                if (Input.GetMouseButtonDown(Mouse.left))
                                    break;
                                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape)) {
                                    selectedNode.Value = originalPosition;
                                    break;
                                }

                                DrawPaths();
                                yield return StateChange.none;
                            }
                        }

                        // insert a node after
                        else {
                            var originalSelectedNode = selectedNode;
                            var newNode = selectedPath.list.AddAfter(selectedNode, mousePosition);
                            selectedNode = newNode;
                            while (TryGetMousePosition(out mousePosition)) {
                                selectedNode.Value = mousePosition;
                                if (Input.GetMouseButtonDown(Mouse.left))
                                    break;
                                if (Input.GetMouseButtonDown(Mouse.right) || Input.GetKeyDown(KeyCode.Escape)) {
                                    selectedPath.list.Remove(selectedNode);
                                    selectedNode = originalSelectedNode;
                                    break;
                                }

                                DrawPaths();
                                yield return StateChange.none;
                            }
                        }
                    }
                }

                while (Game.Instance.TryDequeueCommand(out var command))
                    switch (command) {
                        case (LevelEditorSessionState.SelectModeCommand, _):
                            yield return HandleModeSelectionCommand(command);
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                DrawPaths();
            }
        }
    }

    public override void Exit() {
        gui.layerStack.Pop();
    }

    [Command]
    public static void CreateNewPath(string name) {
        var game = Game.Instance;
        var level = game.TryGetLevel;
        Assert.IsTrue(!level.paths.Exists(path => path.name == name), $"Path with name {name} already exists");
        var newPath = new Level.Path { name = name };
        newPath.list.AddFirst(game.TryGetLevel.view.cameraRig.transform.position.ToVector2Int());
        
        var pathsMode = game.stateMachine.TryFind<LevelEditorPathsModeState>();
        level.paths.Add(newPath);
        if (pathsMode != null) {
            pathsMode.selectedPath = newPath;
            pathsMode.selectedNode = newPath.list.First;
        }
    }

    [Command]
    public static void MoveUnitAlongPath(string pathName) {
        var game = Game.Instance;
        var level = game.TryGetLevel;
        var path = level.paths.Find(p => p.name == pathName);
        Assert.IsNotNull(path, $"Path with name {pathName} not found");
        var found = level.TryGetUnit(path.list.First.Value, out var unit);
        Assert.IsTrue(found, $"Unit not found at {path.list.First.Value}");
        var unitView = unit.view;
        Assert.IsTrue(unitView);
        unitView.Position = path.list.First.Value;
        var pathList = new List<Vector2Int> { path.list.First.Value };
        for (var node = path.list.First.Next; node != null; node = node.Next)
            pathList.AddRange(Woo.Traverse2D(pathList[^1], node.Value));
        var oldEnableDance = unitView.enableDance;
        unitView.enableDance = false;
        var animation = new MoveSequence(unitView.transform, pathList, unitSpeed, onComplete: () => unitView.enableDance = oldEnableDance).Animation();
        unitView.StartCoroutine(animation);
    }
}