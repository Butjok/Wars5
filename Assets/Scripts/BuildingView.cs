using System;
using UnityEngine;

public class BuildingView : MonoBehaviour {

    public static BuildingView GetPrefab(TileType type) {
        return type switch {
            TileType.City or TileType.Hq or TileType.Factory  => "WbFactory".LoadAs<BuildingView>(),
            TileType.MissileSilo => "WbMissileSilo".LoadAs<BuildingView>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public BuildingView prefab;
    public Building building;
    public MeshRenderer[] renderers = { };
    public MaterialPropertyBlock materialPropertyBlock;
    public void Awake() {
        materialPropertyBlock = new MaterialPropertyBlock();
    }
    public void Reset() {
        renderers = GetComponentsInChildren<MeshRenderer>();
    }
    public Vector2Int Position {
        get => transform.position.ToVector2().RoundToInt();
        set => transform.position = value.Raycast();
    }
    virtual public Vector2Int LookDirection {
        get => transform.forward.ToVector2().RoundToInt();
        set => transform.rotation = Quaternion.LookRotation(value.ToVector3Int(), Vector3.up);
    }
    public Color PlayerColor {
        set {
            materialPropertyBlock.SetColor("_PlayerColor", value);
            foreach (var renderer in renderers)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
    public bool Moved {
        set {
            materialPropertyBlock.SetFloat("_Moved", value ? 1 : 0);
            foreach (var renderer in renderers)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}