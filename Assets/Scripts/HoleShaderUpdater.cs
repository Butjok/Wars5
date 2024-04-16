using System;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;
using Random = UnityEngine.Random;

public class HoleShaderUpdater : MonoBehaviour {

    public enum Mode { PlaceInstances, PlaceHoles }

    public Camera camera;
    public MeshRenderer prefab;
    public Dictionary<Vector2Int, MeshRenderer> instances = new();
    public Material[] materials = Array.Empty<Material>();

    public HashSet<Vector2Int> holes = new();
    public HashSet<Vector2Int> oldHoles = new();
    public Mode mode;

    public bool enableInput = false;
    public bool drawHoles = false;
    public Color holeColor = Color.yellow;

    public List<UnitView> unitViews = new();

    public bool TryRemoveInstance(Vector2Int position) {
        if (instances.TryGetValue(position, out var instance))
            Destroy(instance.gameObject);
        return instances.Remove(position);
    }

    public void Update() {

        if (enableInput) {
            if (Input.GetKeyDown(KeyCode.Tab))
                mode = mode == Mode.PlaceInstances ? Mode.PlaceHoles : Mode.PlaceInstances;

            var left = Input.GetMouseButton(Mouse.left);
            var right = Input.GetMouseButton(Mouse.right);
            if ((left || right) && camera.TryRaycastPlane(out var hitPoint)) {
                var position = hitPoint.ToVector2Int();
                if (mode == Mode.PlaceInstances) {
                    TryRemoveInstance(position);
                    if (left) {
                        var instance = Instantiate(prefab, position.ToVector3() + Random.value * Vector3.up, Quaternion.identity);
                        instances[position] = instance;
                    }
                }
                else {
                    if (left)
                        holes.Add(position);
                    else
                        holes.Remove(position);
                }
            }
        }

        if (drawHoles)
            foreach (var position in holes)
                Draw.ingame.CircleXZ(position.ToVector3(), .33f, holeColor);

        /*holes.Clear();
        holes.UnionWith(unitViews.Select(uv => uv.body.transform.position.ToVector2Int()));

        oldHoles.SymmetricExceptWith(holes);
        var holesWereModified = oldHoles.Count > 0;
        if (holesWereModified)
            UpdateTexture(unitViews.Select(uv => (uv.body.transform.position.ToVector2Int(), uv.PlayerColor)));

        oldHoles.Clear();
        oldHoles.UnionWith(holes);*/
    }

    public Texture2D clearTexture;
    public Texture2D texture;

    public void Awake() {
        clearTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
        clearTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
        clearTexture.Apply();
        ResetTexture();
    }
    
    public Texture2D UpdateTexture(IEnumerable<(Vector2Int position, Color color)> items) {
        var colors = items.ToDictionary(i => i.position, i => i.color);
        if (colors.Count == 0)
            return ResetTexture();
        var (texture, transform) = TileMask.Create(colors.Keys, color: colors.GetValueOrDefault);
        this.texture = texture;
        Shader.SetGlobalTexture("_HoleMask", texture);
        Shader.SetGlobalMatrix("_HoleMask_WorldToLocal", transform.inverse);
        return texture;
    }
    public Texture2D ResetTexture() {
        texture = clearTexture;
        Shader.SetGlobalTexture("_HoleMask", clearTexture);
        Shader.SetGlobalMatrix("_HoleMask_WorldToLocal", Matrix4x4.identity);
        return texture;
    }

    private void OnGUI() {
        if (enableInput) {
            GUI.skin = DefaultGuiSkin.TryGet;
            GUILayout.Label($"Mode: {mode}");
        }
    }
}