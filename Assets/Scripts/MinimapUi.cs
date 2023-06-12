using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Color = UnityEngine.Color;

// For a discussion of the code, see: https://www.hallgrimgames.com/blog/2018/11/25/custom-unity-ui-meshes

public class MinimapUi : MaskableGraphic {
    
    [SerializeField]
    Texture m_Texture;

    // make it such that unity will trigger our ui element to redraw whenever we change the texture in the inspector
    public Texture texture {
        get {
            return m_Texture;
        }
        set {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }

    // if no texture is configured, use the default white texture as mainTexture
    public override Texture mainTexture {
        get {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    // helper to easily create quads for our ui mesh. You could make any triangle-based geometry other than quads, too!
    void AddQuad(VertexHelper vertexHelper, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2, Color color) {
        var firstIndex = vertexHelper.currentVertCount;

        var vertex = new UIVertex();
        vertex.color = color; // Do not forget to set this, otherwise 

        vertex.position = corner1;
        vertex.uv0 = uvCorner1;
        vertexHelper.AddVert(vertex);

        vertex.position = new Vector2(corner2.x, corner1.y);
        vertex.uv0 = new Vector2(uvCorner2.x, uvCorner1.y);
        vertexHelper.AddVert(vertex);

        vertex.position = corner2;
        vertex.uv0 = uvCorner2;
        vertexHelper.AddVert(vertex);

        vertex.position = new Vector2(corner1.x, corner2.y);
        vertex.uv0 = new Vector2(uvCorner1.x, uvCorner2.y);
        vertexHelper.AddVert(vertex);

        vertexHelper.AddTriangle(firstIndex + 0, firstIndex + 2, firstIndex + 1);
        vertexHelper.AddTriangle(firstIndex + 3, firstIndex + 2, firstIndex + 0);
    }

    public Vector2 unitSize = new(50, 50);
    private readonly Dictionary<Vector2Int, (TileType type, Color playerColor)> tiles = new();

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

    public void ReplaceTiles(IEnumerable<( Vector2Int position, TileType type, Color playerColor)> tiles) {
        this.tiles.Clear();
        foreach (var (position, type, playerColor) in tiles)
            this.tiles.Add(position, (type, playerColor));
        SetVerticesDirty();
    }

    public TileTypeSpriteDictionary atlas = new();

    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        if (tiles.Count == 0)
            return;

        var min = tiles.Keys.Aggregate(new Vector2Int(int.MaxValue, int.MaxValue), Vector2Int.Min);
        var max = tiles.Keys.Aggregate(new Vector2Int(int.MinValue, int.MinValue), Vector2Int.Max);
        var count = max - min + Vector2Int.one;
        var size = count * unitSize;
        var startOffset = -size / 2;

        for (var y = min.y; y <= max.y; y++)
        for (var x = min.x; x <= max.x; x++) {
            var position = new Vector2Int(x, y);
            var index = position - min;
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
            AddQuad(vertexHelper, offset, offset + unitSize, uvMin, uvMax, playerColor);
        }
    }
}