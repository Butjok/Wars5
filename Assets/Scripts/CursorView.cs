using System;
using Butjok.CommandLine;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

    public MeshRenderer meshRenderer;
    [Command] public bool show = true;
    public bool showGui = true;
    public bool showOnlyOnTiles = true;
    public LevelView levelView;

    public Func<Vector2Int, bool> hasTile;

    public void Reset() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void LateUpdate() {
        if (levelView && levelView.cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition) && (!showOnlyOnTiles || (hasTile?.Invoke(mousePosition) ?? false))) {
            meshRenderer.enabled = show;
            transform.position = mousePosition.ToVector3Int();
        }
        else {
            meshRenderer.enabled = false;
        }
    }

    private void OnGUI() {
        if (showGui && levelView && levelView.cameraRig.camera.TryGetMousePosition(out Vector2Int position)) {
            GUI.skin = DefaultGuiSkin.TryGet;
            var content = new GUIContent(position.ToString());
            var size = GUI.skin.label.CalcSize(content);
            GUI.Label(new Rect(Screen.width - size.x, Screen.height - size.y, size.x, size.y), position.ToString());
        }
    }
}