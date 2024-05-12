using UnityEngine;

/*
 * NOTE: All os these should become obsolete at some point and use Unity's Input System instead.
 * 12.6.23
 */

public static class CameraExtensions {

    public static int raycastLayerMask = 1 << LayerMask.NameToLayer("Terrain");

    public static Ray FixedScreenPointToRay(this Camera camera, Vector3 position) {
        camera.ResetProjectionMatrix();
        var m = camera.projectionMatrix;
        m.m11 *= CameraRig.verticalStretch;
        camera.projectionMatrix = m;

        position /= new Vector2(camera.pixelWidth, camera.pixelHeight);

        position.x = (position.x - 0.5f) * 2f;
        position.y = (position.y - 0.5f) * 2f;
        position.z = -1f;

        var viewportToWorldMatrix = (camera.projectionMatrix * camera.worldToCameraMatrix).inverse;

        var rayOrigin = viewportToWorldMatrix.MultiplyPoint(position);

        var endPosition = position;
        endPosition.z = 1f;
        var rayEnd = viewportToWorldMatrix.MultiplyPoint(endPosition);

        return new Ray(rayOrigin, rayEnd - rayOrigin);
    }

    public static bool TryGetMousePosition(this Camera camera, out RaycastHit hit) {
        if (!camera) {
            hit = default;
            return false;
        }
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, float.MaxValue, raycastLayerMask);
    }
    public static bool TryGetMousePosition(this Camera camera, out Vector3 point) {
        if (!camera) {
            point = default;
            return false;
        }
        if (TryGetMousePosition(camera, out RaycastHit hit)) {
            point = hit.point;
            return true;
        }
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(camera.ScreenPointToRay(Input.mousePosition), out var distance)) {
            point = camera.ScreenPointToRay(Input.mousePosition).GetPoint(distance);
            return true;
        }
        point = default;
        return false;
    }
    public static bool TryGetMousePosition(this Camera camera, out Vector2Int position) {
        if (TryGetMousePosition(camera, out Vector3 point)) {
            position = point.ToVector2Int();
            return true;
        }
        position = default;
        return false;
    }

    public static bool TryCalculateScreenCircle(this Camera camera, Vector3 position, float radius, out Vector3 center, out float halfSize) {
        var screenPoint0 = camera.WorldToScreenPoint(position);
        var screenPoint1 = camera.WorldToScreenPoint(position + camera.transform.right * radius);
        if (screenPoint0.z > 0 && screenPoint1.z > 0) {
            center = screenPoint0;
            halfSize = (screenPoint0 - screenPoint1).magnitude;
            return true;
        }

        center = default;
        halfSize = default;
        return false;
    }
}