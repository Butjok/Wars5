using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;

// For a discussion of the code, see: https://www.hallgrimgames.com/blog/2018/11/25/custom-unity-ui-meshes

public class MinimapUi : MaskableGraphic {

    public Texture texture;
    public override Texture mainTexture => texture ? texture : s_WhiteTexture;

    public CameraFrustumUi cameraFrustumUi;

    public Vector2 unitSize = new(50, 50);
    private readonly Dictionary<Vector2Int, (TileType type, Color playerColor)> tiles = new();

    public RectTransform scalingRoot;
    public Vector2 scalingBounds = new(.5f,2);

    public TextAsset saveFile;
    protected override void Start() {

        if (!saveFile)
            return;

        var stack = new Stack();
        var tiles = new List<(Vector2Int Position, TileType type, Color playerColor)>();
        ColorName? playerColor = null;

        foreach (var token in Tokenizer.Tokenize(saveFile.text))
            switch (token) {
                case "tile.add": {
                    var position = (Vector2Int)stack.Pop();
                    var type = (TileType)stack.Pop();
                    tiles.Add((position, type, Color.white));
                    break;
                }
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        ReplaceTiles(tiles);
    }

    private void Update() {
        var value = Input.GetAxisRaw("Mouse ScrollWheel");
        if (value != 0) {
            var scale = Mathf.Clamp(scalingRoot.localScale.x * (1 + value), scalingBounds[0], scalingBounds[1]);
            scalingRoot.localScale = new Vector3(scale,scale,1);
        }
    }

    public void ReplaceTiles(IEnumerable<( Vector2Int position, TileType type, Color playerColor)> input) {

        tiles.Clear();
        foreach (var (position, type, playerColor) in input)
            tiles.Add(position, (type, playerColor));

        bounds.min = tiles.Keys.Aggregate(new Vector2Int(int.MaxValue, int.MaxValue), Vector2Int.Min);
        bounds.max = tiles.Keys.Aggregate(new Vector2Int(int.MinValue, int.MinValue), Vector2Int.Max);
        if (cameraFrustumUi) {
            cameraFrustumUi.unitSize = unitSize;
            cameraFrustumUi.worldBounds.min = bounds.min - Vector2.one / 2;
            cameraFrustumUi.worldBounds.max = bounds.max + Vector2.one / 2;
        }

        SetVerticesDirty();
    }


    public TileTypeSpriteDictionary atlas = new();
    public RectInt bounds;
    public Vector2Int Count => bounds.max - bounds.min + Vector2Int.one;

    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        if (tiles.Count == 0)
            return;

        var size = Count * unitSize;
        var startOffset = -size / 2;

        for (var y = bounds.min.y; y <= bounds.max.y; y++)
        for (var x = bounds.min.x; x <= bounds.max.x; x++) {
            var position = new Vector2Int(x, y);
            var index = position - bounds.min;
            if (!tiles.TryGetValue(position, out var tuple))
                continue;
            var (tileType, playerColor) = tuple;
            var offset = startOffset + unitSize * index;
            var uvMin = Vector2.zero;
            var uvMax = Vector2.one;
            if (atlas.TryGetValue(tileType, out var sprite)) {
                var textureSize = new Vector2Int(sprite.texture.width, sprite.texture.height);
                uvMin = sprite ? sprite.rect.min / textureSize : Vector2.zero;
                uvMax = sprite ? sprite.rect.max / textureSize : Vector2.one;
            }
            vertexHelper.AddRect(new Rect(offset, unitSize), new Rect(uvMin, uvMax - uvMin), new Rect(index, Vector2Int.one), playerColor);
        }
    }
    
}

public static class VertexHelperExtensions {

    public static void AddQuad(this VertexHelper vertexHelper, UIVertex a, UIVertex b, UIVertex c, UIVertex d) {
        var firstIndex = vertexHelper.currentVertCount;
        vertexHelper.AddVert(a);
        vertexHelper.AddVert(b);
        vertexHelper.AddVert(c);
        vertexHelper.AddVert(d);
        vertexHelper.AddTriangle(firstIndex + 0, firstIndex + 1, firstIndex + 2);
        vertexHelper.AddTriangle(firstIndex + 2, firstIndex + 3, firstIndex + 0);
    }

    public const int min = 0, max = 1;
    public static void AddRect(this VertexHelper vertexHelper, Rect position, Rect uv0, Rect uv1, Color color) {
        UIVertex V(int x, int y) => new() {
            position = new Vector2(x == min ? position.xMin : position.xMax, y == min ? position.yMin : position.yMax),
            uv0 = new Vector2(x == min ? uv0.xMin : uv0.xMax, y == min ? uv0.yMin : uv0.yMax),
            uv1 = new Vector2(x == min ? uv1.xMin : uv1.xMax, y == min ? uv1.yMin : uv1.yMax),
            color = color
        };
        vertexHelper.AddQuad(V(min, min), V(min, max), V(max, max), V(max, min));
    }
}