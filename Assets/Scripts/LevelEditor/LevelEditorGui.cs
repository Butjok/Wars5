using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorGui : MonoBehaviour {

    public int depth = -1000;
    public Stack<Action> layerStack = new();

    private void OnGUI() {
        
        GUI.skin = DefaultGuiSkin.TryGet;
        GUI.depth = depth;

        if (layerStack.TryPeek(out var action))
            action();
    }
}