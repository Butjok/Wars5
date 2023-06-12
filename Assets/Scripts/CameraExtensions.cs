using UnityEngine;

/*
 * NOTE: All os these should become obsolete at some point and use Unity's Input System instead.
 * 12.6.23
 */

public static class CameraExtensions {

    public static int raycastLayerMask = (-1) ^ (1 << LayerMask.NameToLayer("UI")) ^ (1 << LayerMask.NameToLayer("Ignore Raycast"));
    
    public static bool TryGetMousePosition(this Camera camera, out RaycastHit hit) {
        if (!camera) {
            hit = default;
            return false;
        }
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, float.MaxValue, raycastLayerMask);
    }
    
    public static bool TryGetMousePosition(this Camera camera, out RaycastHit hit, out Vector2Int position) {
        position = default;
        if (!camera.TryGetMousePosition(out hit))
            return false;
        position = hit.point.ToVector2Int();
        return true;
    }

    public static bool TryGetMousePosition(this Camera camera, out Vector2Int position) {
        position = default;
        if (!camera.TryGetMousePosition(out RaycastHit hit))
            return false;
        position = hit.point.ToVector2Int();
        return true;
    }
    
    public static bool TryGetMousePosition(this Camera camera, out Vector3 position) {
        position = default;
        if (!camera.TryGetMousePosition(out RaycastHit hit))
            return false;
        position = hit.point;
        return true;
    }

    // public static bool TryGetMousePosition(this Camera camera, out Vector3 position) {
    //     return camera.TryGetMousePosition(out position, out _);
    // }
    //
    // public static bool TryGetMousePosition(this Camera camera, out Vector2 position) {
    //     position = default;
    //     if (camera.TryGetMousePosition(out Vector3 vector3Position)) {
    //         position = vector3Position.ToVector2();
    //         return true;
    //     }
    //     return false;
    // }
    //
    // public static bool TryGetMousePosition(this Camera camera, out Vector2Int position) {
    //     position = default;
    //     if (camera.TryGetMousePosition(out Vector3 vector3Position)) {
    //         position = vector3Position.ToVector2().RoundToInt();
    //         return true;
    //     }
    //     return false;
    // }
}