using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;

public class LevelEditorGui : MonoBehaviour {

    public int depth = -1000;
    public Stack<Action> layerStack = new();
    
    public List<(string message, float startTime, float duration)> notifications = new();

    private void OnGUI() {
        
        GUI.skin = DefaultGuiSkin.TryGet;
        GUI.depth = depth;

        if (layerStack.TryPeek(out var action))
            action();
        
        // Draw notifications from bottom to top on yellow background with black text
        var style =  GUI.skin.GetStyle("Notification");
        var height = style.CalcHeight(new GUIContent("Test"), Screen.width);
        var y = Screen.height - height;
        foreach (var (message, _, _) in notifications) {
            var rect = new Rect(0, y, Screen.width, height);
            GUI.Label(rect, message, GUI.skin.GetStyle("Notification"));
            y -= height;
        }
        
        // remove old notifications
        var time = Time.time;
        notifications.RemoveAll(notification => time - notification.startTime > notification.duration);
    }
    
    [Command]
    public void AddNotification(string message, float duration = 2) {
        notifications.Add((message, Time.time, duration));
    }
}