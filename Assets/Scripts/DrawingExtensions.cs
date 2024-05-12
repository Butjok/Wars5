using Drawing;
using UnityEngine;

public static class DrawingExtensions {
    public static Vector3 Raycasted(this Vector2Int position) {
        if (position.TryRaycast(out var hit))
            return hit.point;
        return position.ToVector3();
    }
}