using UnityEngine;
using UnityEngine.Assertions;

public static class Mouse
{
    public const int left = 0;
    public const int right = 1;
    public const int middle = 2;

    public static bool TryGetPosition(out Vector3 position, out RaycastHit hit) {
        
        position = default;
        hit = default;
        
        var camera = Camera.main;
        if (!camera)
            return false;
        
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMasks.Terrain)) {
            position = hit.point;
            return true;
        }
        
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray,out var enter)) {
            position = ray.GetPoint(enter);
            return true;
        }
        
        return false;
    }
    
    public static bool TryGetPosition(out Vector3 position) {
        return TryGetPosition(out position, out _);
    }
    
    public static bool TryGetPosition(out Vector2 position) {
        position = default;
        if (TryGetPosition(out Vector3 vector3Position)) {
            position = vector3Position.ToVector2();
            return true;    
        }
        return false;
    }
    
    public static bool TryGetPosition(out Vector2Int position) {
        position = default;
        if (TryGetPosition(out Vector3 vector3Position)) {
            position = vector3Position.ToVector2().RoundToInt();
            return true;    
        }
        return false;
    }
}