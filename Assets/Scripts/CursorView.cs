using System;
using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

    private struct Context {
        public bool canBeVisible;
        public bool hideOnPop;
    }
    private Stack<Context> contexts = new();
    public void PushContext(bool canBeVisible, bool hideOnPop) {
        contexts.Push(new Context() { canBeVisible = canBeVisible, hideOnPop = hideOnPop });
    }
    public void PopContext() {
        Assert.IsTrue(contexts.TryPeek(out var scope));
        if (scope.hideOnPop)
            TryHide();
        contexts.Pop();
    }

    public Vector2Int? Position {
        get => renderer.enabled ? transform.position.ToVector2Int() : null;
        set {
            
        }
    }

    public bool ShowAt(Vector2Int position) {
        if (contexts.TryPeek(out var scope) && !scope.canBeVisible)
            return false;
        
        if (!Visible)
            return TryShow(position);
        if (Position != position) {
            Position = position;
            return true;
        }
        return false;
    }
    
    public bool TryHide() {
        if (contexts.TryPeek(out var scope) && !scope.canBeVisible)
            return false;
        
        if (!Visible)
            return false;
        Visible = false;
        return true;
    }

    public Renderer renderer;
    public bool Visible {
        get => renderer.enabled;
        private set => renderer.enabled = value;
    }

    public void Reset() {
        // meshRenderer = GetComponent<MeshRenderer>();
    }

    public void LateUpdate() {
        // if (levelView && levelView.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && (!showOnlyOnTiles || (hasTile?.Invoke(mousePosition) ?? false))) {
        //     meshRenderer.enabled = show;
        //     transform.position = mousePosition.ToVector3Int();
        // }
        // else {
        //     meshRenderer.enabled = false;
        // }
    }

    private void OnGUI() {
        // if (showGui && levelView && levelView.cameraRig.camera.TryGetMousePosition(out Vector2Int position)) {
        //     GUI.skin = DefaultGuiSkin.TryGet;
        //     var content = new GUIContent(position.ToString());
        //     var size = GUI.skin.label.CalcSize(content);
        //     GUI.Label(new Rect(Screen.width - size.x, Screen.height - size.y, size.x, size.y), position.ToString());
        // }
    }
}