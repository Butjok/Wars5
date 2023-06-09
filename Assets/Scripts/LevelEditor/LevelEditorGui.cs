using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelEditorGui : MonoBehaviour {

    public int depth = -1000;
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
        if (infosStack.TryPeek(out var info)) {

            var groupedKeys = info.Keys
                .OrderBy(key => key)
                .GroupBy(key => !key.Contains('.') ? "" : key.Substring(0, key.LastIndexOf('.')));

            foreach (var group in groupedKeys)
                if (group.Count() == 1) {
                    var key = group.First();
                    GUILayout.Label($"{key}: {info[key]()}");
                }
                else {
                    var padding = group.Key == "" ? "" : "  "; 
                    if (group.Key != "")
                        GUILayout.Label(group.Key);
                    foreach (var key in group)
                        GUILayout.Label($"{padding}{key.Split('.').Last()}: {info[key]()}");
                    GUILayout.Space(4);
                }
        }
    }
    public void Remove(Func<string, bool> condition) {
        var peeked = infosStack.TryPeek(out var info);
        Assert.IsTrue(peeked);
        foreach (var key in info.Keys.Where(condition).ToList())
            info.Remove(key);
    }
    public void Remove(string key) {
        Remove(k => k == key);
    }
}