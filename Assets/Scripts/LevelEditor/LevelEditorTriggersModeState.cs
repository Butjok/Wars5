using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditorTriggersModeState : StateMachineState {

    public enum Command { CycleTrigger, PlaceTrigger, RemoveTrigger, PickTrigger }

    public TriggerName[] triggerNames = { TriggerName.A, TriggerName.B, TriggerName.C, TriggerName.D, TriggerName.E, TriggerName.F };
    public TriggerName triggerName = TriggerName.A;

    public float offset = .01f;
    public Dictionary<TriggerName,Color> triggerColors = new() {
        [TriggerName.A] = Color.red,
        [TriggerName.B] = Color.green,
        [TriggerName.C] = Color.blue,
        [TriggerName.D] = Color.cyan,
        [TriggerName.E] = Color.yellow,
        [TriggerName.F] = Color.magenta
    };
    
    public LevelEditorTriggersModeState(StateMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator<StateChange> Entry {
        get {
            var (game, editorState) = (FindState<GameSessionState>().game, FindState<LevelEditorSessionState>());
            var level = editorState.level;
            var gui = editorState.gui;
            var triggers = level.triggers;
            var camera = level.view.cameraRig.camera;
            
            gui
                .Push()
                .Add("TriggerName", () => triggerName);

            while (true) {
                yield return StateChange.none;
                
                editorState.DrawBridges();

                if (Input.GetKeyDown(KeyCode.F8))
                    game.EnqueueCommand(LevelEditorSessionState.Command.SelectTilesMode);

                else if (Input.GetKeyDown(KeyCode.Tab))
                    game.EnqueueCommand(Command.CycleTrigger, Input.GetKey(KeyCode.LeftShift) ? -1 : 1);

                else if (Input.GetMouseButton(Mouse.left) && camera.TryGetMousePosition(out Vector2Int mousePosition))
                    game.EnqueueCommand(Command.PlaceTrigger, mousePosition);

                else if (Input.GetMouseButton(Mouse.right) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.RemoveTrigger, mousePosition);

                else if (Input.GetKeyDown(KeyCode.F5))
                    game.EnqueueCommand(LevelEditorSessionState.Command.Play);

                else if (Input.GetKeyDown(KeyCode.LeftAlt) && camera.TryGetMousePosition(out mousePosition))
                    game.EnqueueCommand(Command.PickTrigger, mousePosition);
                
                while (game.TryDequeueCommand(out var command))
                    switch (command) {

                        case (LevelEditorSessionState.Command.SelectTilesMode, _):
                            yield return StateChange.ReplaceWith(new LevelEditorTilesModeState(stateMachine));
                            break;

                        case (Command.CycleTrigger, int offset):
                            triggerName = triggerName.Cycle(triggerNames, offset);
                            Assert.IsTrue(triggers.ContainsKey(triggerName));
                            break;

                        case (Command.PlaceTrigger, Vector2Int position):
                            triggers[triggerName].Add(position);
                            break;

                        case (Command.RemoveTrigger, Vector2Int position):
                            foreach (var (_, set) in triggers)
                                set.Remove(position);
                            break;

                        case (LevelEditorSessionState.Command.Play, _):
                            yield return StateChange.Push(new LevelEditorPlayState(stateMachine));
                            break;

                        case (Command.PickTrigger, Vector2Int position):
                            var candidates = triggers.Where(kv => kv.Value.Contains(position)).Select(kv => kv.Key).ToArray();
                            if (candidates.Length > 0)
                                triggerName = triggerName.Cycle(candidates, 1);
                            break;

                        default:
                            HandleUnexpectedCommand(command);
                            break;
                    }

                var positions = triggers.SelectMany(t => t.Value).Distinct();
                foreach (var position in positions) {
                    
                    var names = triggers.Keys.Where(t => triggers[t].Contains(position)).ToArray();
                    var color = Color.black;
                    foreach (var name in names) {
                        if (triggerColors.TryGetValue(name, out var triggerColor))
                            color += triggerColor;
                    }

                    Draw.ingame.SolidPlane(position.ToVector3Int() + Vector3.up * offset, Vector3.up, Vector2.one, color);
                    Draw.ingame.Label2D((Vector3)position.ToVector3Int(), string.Join(",", names), Color.white);
                }
            }
        }
    }

    public override void Exit() {
        stateMachine.TryFind<LevelEditorSessionState>().gui.Pop();
    }
}