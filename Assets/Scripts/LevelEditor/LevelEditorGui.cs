using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditorGui : MonoBehaviour {

    public int depth = -2000;
    private Stack<Dictionary<string, Func<object>>> infosStack = new();

    public LevelEditorGui Add(string key, Func<object> getter, bool isPermanent = false) {
        Assert.IsTrue(infosStack.Count > 0);
        infosStack.Peek()[key] = getter;
        return this;
    }
    public LevelEditorGui Push() {
        infosStack.Push(new Dictionary<string, Func<object>>());
        return this;
    }
    public void Pop() {
        infosStack.Pop();
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUI.depth = depth;

        GUILayout.Space(25);
        if (infosStack.TryPeek(out var info))
            foreach (var (key, getter) in info.OrderBy(pair => pair.Key))
                GUILayout.Label($"{key}: {getter()}");
    }
}