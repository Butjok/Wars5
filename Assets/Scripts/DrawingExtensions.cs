using UnityEngine;

public static class DrawingExtensions {
    public static Vector3 Raycasted(this Vector2Int position) {
        return Raycasted((Vector2)position);
    }
    public static Vector3 Raycasted(this Vector2 position) {
        if (position.TryRaycast(out var hit))
            return hit.point;
        return position;
    }
}