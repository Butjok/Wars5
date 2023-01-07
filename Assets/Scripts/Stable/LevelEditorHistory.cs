using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorHistory {

    private Stack<LevelEditorHistoryAction> undos = new();
    private Stack<LevelEditorHistoryAction> redos = new();

    public bool log = false;

    public void Execute(LevelEditorHistoryAction action) {
        action.commit();
        redos.Clear();
        undos.Push(action);
        if (log)
            Debug.Log($"EXECUTED {action}");
    }
    public void Execute(Action execute, Action revert, string name = null) {
        Execute(new LevelEditorHistoryAction(execute, revert, name));
    }
    public bool TryUndo() {
        if (undos.Count == 0)
            return false;
        var action = undos.Pop();
        redos.Push(action);
        action.revert();
        if (log)
            Debug.Log($"UNDONE {action}");
        return true;
    }
    public bool TryRedo() {
        if (redos.Count == 0)
            return false;
        var action = redos.Pop();
        undos.Push(action);
        action.commit();
        if (log)
            Debug.Log($"REDONE {action}");
        return true;
    }

    public LevelEditorHistoryCompoundActionBuilder CreateCompoundActionBuilder(string name = null) => new(this, name);
}

public class LevelEditorHistoryAction {

    public Action commit, revert;
    public string name;

    public LevelEditorHistoryAction(Action commit, Action revert, string name = null) {
        this.commit = commit;
        this.revert = revert;
        this.name = name;
    }

    public override string ToString() => name;
}

public class LevelEditorHistoryCompoundActionBuilder {

    private readonly LevelEditorHistory history;
    private readonly string name;

    private readonly Queue<Action> commits = new();
    private readonly Stack<Action> reverts = new();

    public LevelEditorHistoryCompoundActionBuilder(LevelEditorHistory history, string name = null) {
        this.history = history;
        this.name = name;
    }

    public LevelEditorHistoryAction ToAction {
        get {
            return new LevelEditorHistoryAction(
                () => {
                    foreach (var action in commits)
                        action();
                },
                () => {
                    foreach (var action in reverts)
                        action();
                },
                name);
        }
    }

    public LevelEditorHistoryCompoundActionBuilder EnqueueCommitAction(Action action) {
        commits.Enqueue(action);
        return this;
    }
    public LevelEditorHistoryCompoundActionBuilder PushRevertAction(Action action) {
        reverts.Push(action);
        return this;
    }

    public void Execute() {
        
        // don't execute empty actions
        if (commits.Count == 0 || reverts.Count == 0)
            return;
        
        history.Execute(ToAction);
    }
}