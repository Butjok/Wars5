using System;
using UnityEngine;

public class BuildingView : MonoBehaviour {

    public static BuildingView GetPrefab(TileType type) {
        return type switch {
            TileType.City or TileType.Factory=>"City".LoadAs<BuildingView>(),
            TileType.Hq => "Hq".LoadAs<BuildingView>(),
            TileType.MissileSilo => "WbMissileSilo".LoadAs<BuildingView>(),
            _ => "WbFactory".LoadAs<BuildingView>()
        };
    }
    
    public static Color unownedColor = new(.75f, .66f, .45f);
    public static Color unownedLightsColor = Color.black;

    public BuildingView prefab;
    public Building building;
    public MeshRenderer[] renderers = { };
    public MaterialPropertyBlock materialPropertyBlock;
    public Light[] lights = Array.Empty<Light>();

    public void Awake() {
        materialPropertyBlock = new MaterialPropertyBlock();
        lights = GetComponentsInChildren<Light>();
    }

    public void Reset() {
        renderers = GetComponentsInChildren<MeshRenderer>();
        lights = GetComponentsInChildren<Light>();
    }

    public Vector2Int Position {
        get => transform.position.ToVector2().RoundToInt();
        set {
            if (value.TryRaycast(out var hit))
                transform.position = hit.point;
        }
    }

    public virtual Vector2Int LookDirection {
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

    public Color LightsColor {
        set {
            foreach (var light in lights)
                if (light.name.StartsWith("Player"))
                    light.color = value;
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