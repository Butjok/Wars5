using UnityEngine;

public static class CameraExtensions {

    public static bool TryGetMousePosition(this Camera camera, out Vector3 position, out RaycastHit hit) {

        position = default;
        hit = default;

        if (!camera)
            return false;

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMasks.Terrain)) {
            position = hit.point;
            return true;
        }

        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out var enter)) {
            position = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    public static bool TryGetMousePosition(this Camera camera, out Vector3 position) {
        return camera.TryGetMousePosition(out position, out _);
    }

    public static bool TryGetMousePosition(this Camera camera, out Vector2 position) {
        position = default;
        if (camera.TryGetMousePosition(out Vector3 vector3Position)) {
            position = vector3Position.ToVector2();
            return true;
        }
        return false;
    }

    public static bool TryGetMousePosition(this Camera camera, out Vector2Int position) {
        position = default;
        if (camera.TryGetMousePosition(out Vector3 vector3Position)) {
            position = vector3Position.ToVector2().RoundToInt();
            return true;
        }
        return false;
    }
}