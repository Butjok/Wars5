using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

    public Renderer renderer;
    public Vector2Int? Position {
        get => renderer.enabled ? transform.position.ToVector2Int() : null;
        set {
            renderer.enabled = value != null;
            if (value is { } position)
                transform.position = position.ToVector3Int();
        }
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