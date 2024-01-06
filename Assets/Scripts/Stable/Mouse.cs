using System.Web.UI.WebControls;
using UnityEngine;

public static class Mouse {

    public const int left = 0;
    public const int right = 1;
    public const int middle = 2;
    public const int extra0 = 3;
    public const int extra1 = 4;
    public const int extra2 = 5;

    public static LayerMask DefaultLayerMask => LayerMasks.Terrain | LayerMasks.Roads;

    public static bool TryPhysicsRaycast(this Camera camera, out RaycastHit hit) {
        return TryPhysicsRaycast(camera, out hit, DefaultLayerMask);
    }

    public static bool TryPhysicsRaycast(this Camera camera, out RaycastHit hit, LayerMask layerMask) {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask);
    }

    public static bool TryPhysicsRaycast(this Camera camera, out Vector3 point, LayerMask layerMask) {
        if (camera.TryPhysicsRaycast(out RaycastHit hit, layerMask)) {
            point = hit.point;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    public static bool TryPhysicsRaycast(this Camera camera, out Vector3 point) {
        return TryPhysicsRaycast(camera, out point, DefaultLayerMask);
    }

    public static bool TryRaycastPlane(this Camera camera, out Vector3 point, float planeHeight = 0) {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, Vector3.up * planeHeight);
        if (plane.Raycast(ray, out var distance)) {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}