using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class MinimapUi : MaskableGraphic {

    public GameObject root;

    public Texture texture;
    public override Texture mainTexture => texture ? texture : s_WhiteTexture;

    public CameraFrustumUi cameraFrustumUi;

    public Vector2 unitSize = new(50, 50);
    private readonly Dictionary<Vector2Int, (TileType type, Color playerColor)> tiles = new();

    public RectTransform scalingRoot;
    public Vector2 scalingBounds = new(.5f, 2);

    public Image underlayImage;
    public Vector2 underlayPadding = new(10, 10);

    public Transform cameraRig;
    public RectTransform rotationRoot;

    // public TextAsset saveFile;
    // protected override void Start() {
    //
    //     if (!saveFile)
    //         return;
    //
    //     var stack = new Stack();
    //     var tiles = new List<(Vector2Int Position, TileType type, Color playerColor)>();
    //     ColorName? playerColor = null;
    //
    //     foreach (var token in Tokenizer.Tokenize(saveFile.text))
    //         switch (token) {
    //             case "tile.add": {
    //                 var position = (Vector2Int)stack.Pop();
    //                 var type = (TileType)stack.Pop();
    //                 tiles.Add((position, type, Color.white));
    //                 break;
    //             }
    //             default:
    //                 stack.ExecuteToken(token);
    //                 break;
    //         }
    //
    //     RebuildTiles(tiles);
    // }

    public bool ShowFrustum {
        set {
            if (cameraFrustumUi)
                cameraFrustumUi.enabled = value;
        }
    }

    private bool rotate = true;

    public bool Rotate {
        set {
            rotate = value;
            if (!value)
                rotationRoot.rotation = Quaternion.identity;
        }
    }

    private void LateUpdate() {
        var value = Input.GetAxisRaw("Mouse ScrollWheel");
        if (value != 0) {
            var scale = Mathf.Clamp(scalingRoot.localScale.x * (1 + value), scalingBounds[0], scalingBounds[1]);
            scalingRoot.localScale = new Vector3(scale, scale, 1);
        }

        if (rotate)
            rotationRoot.rotation = Quaternion.Euler(0, 0, cameraRig.eulerAngles.y);
    }

    public void RebuildTiles(IEnumerable<( Vector2Int position, TileType type, Color playerColor)> tiles) {
        this.tiles.Clear();
        foreach (var (position, type, playerColor) in tiles)
            this.tiles.Add(position, (type, playerColor));

        tileBounds.min = this.tiles.Keys.Aggregate(new Vector2Int(int.MaxValue, int.MaxValue), Vector2Int.Min);
        tileBounds.max = this.tiles.Keys.Aggregate(new Vector2Int(int.MinValue, int.MinValue), Vector2Int.Max);
        
        for (var y = tileBounds.min.y; y <= tileBounds.max.y; y++)
            for (var x = tileBounds.min.x; x <= tileBounds.max.x; x++)
                if (!this.tiles.ContainsKey(new Vector2Int(x, y)))
                    this.tiles.Add(new Vector2Int(x, y), (TileType.Sea, Color.white));
        
        if (cameraFrustumUi) {
            cameraFrustumUi.unitSize = unitSize;
            cameraFrustumUi.worldBounds = tileBounds.ToPreciseBounds();
        }

        if (underlayImage)
            underlayImage.rectTransform.sizeDelta = tileBounds.ToPreciseBounds().size * unitSize + underlayPadding * 2;

        SetVerticesDirty();
    }

    public List<MinimapIcon> icons = new();

    public void ClearIcons() {
        foreach (var icon in icons)
            Destroy(icon.gameObject);
        icons.Clear();
    }

    public void RespawnIcons(IEnumerable<(Transform transform, Sprite sprite, Color color)> targets) {
        ClearIcons();
        foreach (var (targetTransform, sprite, color) in targets) {
            var iconGameObject = new GameObject($"Icon for {targetTransform.name}", typeof(RectTransform), typeof(Image), typeof(MinimapIcon));
            var iconRectTransform = iconGameObject.GetComponent<RectTransform>();
            iconRectTransform.SetParent(transform);
            iconRectTransform.sizeDelta = unitSize;
            iconRectTransform.SetSiblingIndex(0);
            var iconImage = iconGameObject.GetComponent<Image>();
            iconImage.sprite = sprite;
            iconImage.color = color;
            iconImage.raycastTarget = false;
            var minimapIcon = iconGameObject.GetComponent<MinimapIcon>();
            minimapIcon.target = targetTransform;
            minimapIcon.ui = this;
            minimapIcon.worldBounds = tileBounds.ToPreciseBounds();
            icons.Add(minimapIcon);
        }
    }

    public void Show(
        IEnumerable<( Vector2Int position, TileType type, Color playerColor)> tiles,
        IEnumerable<(Transform transform, UnitType type, Color playerColor)> units) {
        root.SetActive(true);
        RebuildTiles(tiles);
        RespawnIcons(units.Select(unit => (unit.transform, TryGetSprite(unit.type), unit.playerColor)));
        LateUpdate();
    }

    public void Hide() {
        root.SetActive(false);
    }

    public RectInt tileBounds;
    public Vector2Int Count => tileBounds.max - tileBounds.min + Vector2Int.one;

    [Header("Tile sprites")]
    public Sprite spritePlain;
    public Sprite spriteRoad;
    public Sprite spriteBridge;
    public Sprite spriteBridgeSea;
    public Sprite spriteSea;
    public Sprite spriteMountain;
    public Sprite spriteForest;
    public Sprite spriteRiver;
    public Sprite spriteBeach;

    [Header("Building sprites")]
    public Sprite spriteCity;
    public Sprite spriteHq;
    public Sprite spriteFactory;
    public Sprite spriteAirport;
    public Sprite spriteShipyard;
    public Sprite spriteMissileSilo;

    [Header("Unit sprites")]
    public Sprite spriteInfantry;
    public Sprite spriteAntiTank;
    public Sprite spriteArtillery;
    public Sprite spriteApc;
    public Sprite spriteRecon;
    public Sprite spriteLightTank;
    public Sprite spriteRockets;
    public Sprite spriteMediumTank;

    public Sprite TryGetSprite(TileType tileType) {
        return tileType switch {
            TileType.Plain => spritePlain,
            TileType.Road => spriteRoad,
            TileType.Bridge => spriteBridge,
            TileType.BridgeSea => spriteBridgeSea,
            TileType.Sea => spriteSea,
            TileType.Mountain => spriteMountain,
            TileType.Forest => spriteForest,
            TileType.River => spriteRiver,
            TileType.Beach => spriteBeach,

            TileType.City => spriteCity,
            TileType.Hq => spriteHq,
            TileType.Factory => spriteFactory,
            TileType.Airport => spriteAirport,
            TileType.Shipyard => spriteShipyard,
            TileType.MissileSilo => spriteMissileSilo,

            _ => null
        };
    }
    public Sprite TryGetSprite(UnitType unitType) {
        return unitType switch {
            UnitType.Infantry => spriteInfantry,
            UnitType.AntiTank => spriteAntiTank,
            UnitType.Artillery => spriteArtillery,
            UnitType.Apc => spriteApc,
            UnitType.Recon => spriteRecon,
            UnitType.LightTank => spriteLightTank,
            UnitType.Rockets => spriteRockets,
            UnitType.MediumTank => spriteMediumTank,
            _ => null
        };
    }

    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        if (tiles.Count == 0)
            return;
        
//        Debug.Log(Count.x * Count.y);

        var size = Count * unitSize;
        var startOffset = -size / 2;

        for (var y = tileBounds.min.y; y <= tileBounds.max.y; y++)
        for (var x = tileBounds.min.x; x <= tileBounds.max.x; x++) {
            var position = new Vector2Int(x, y);
            var index = position - tileBounds.min;
            if (!tiles.TryGetValue(position, out var tuple))
                continue;
            var (tileType, playerColor) = tuple;
            var offset = startOffset + unitSize * index;

            {
                var uvMin = Vector2.zero;
                var uvMax = Vector2.one;
                var tileSprite = TryGetSprite((tileType & TileType.Buildings) != 0 ? TileType.Plain : tileType);
                if (tileSprite) {
                    var textureSize = new Vector2Int(tileSprite.texture.width, tileSprite.texture.height);
                    uvMin = tileSprite ? tileSprite.rect.min / textureSize : Vector2.zero;
                    uvMax = tileSprite ? tileSprite.rect.max / textureSize : Vector2.one;
                }

                vertexHelper.AddRect(new Rect(offset, unitSize), new Rect(uvMin, uvMax - uvMin), new Rect(index, Vector2Int.one), Color.white);
            }

            {
                var uvMin = Vector2.zero;
                var uvMax = Vector2.one;
                if ((tileType & TileType.Buildings) != 0) {
                    var buildingSprite = TryGetSprite(tileType);
                    if (buildingSprite) {
                        var textureSize = new Vector2Int(buildingSprite.texture.width, buildingSprite.texture.height);
                        uvMin = buildingSprite ? buildingSprite.rect.min / textureSize : Vector2.zero;
                        uvMax = buildingSprite ? buildingSprite.rect.max / textureSize : Vector2.one;
                    }

                    vertexHelper.AddRect(new Rect(offset, unitSize), new Rect(uvMin, uvMax - uvMin), new Rect(index, Vector2Int.one), playerColor);
                }
            }
            
//            Debug.Log(vertexHelper.currentVertCount);
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