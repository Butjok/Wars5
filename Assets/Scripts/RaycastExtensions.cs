using UnityEngine;

public static class RaycastExtensions {
    public static bool TryRaycast(this Vector2 position, out RaycastHit hit) {
        return TryRaycast(position, out hit, LayerMasks.Terrain | LayerMasks.Roads);
    }

    public static bool TryRaycast(this Vector2 position, out RaycastHit hit, LayerMask layerMask) {
        return Physics.Raycast(position.ToVector3() + Vector3.up * 100, Vector3.down, out hit, float.MaxValue, layerMask);
    }

    public static bool TryRaycast(this Vector2Int position, out RaycastHit hit) {
        return position.TryRaycast(out hit, LayerMasks.Terrain | LayerMasks.Roads);
    }

    public static bool TryRaycast(this Vector2Int position, out RaycastHit hit, LayerMask layerMask) {
        return ((Vector2)position).TryRaycast(out hit, layerMask);
    }
}