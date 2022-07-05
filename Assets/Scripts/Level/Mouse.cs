using UnityEngine;
using UnityEngine.Assertions;

public static class Masks
{
    public static int selectable = 1 << LayerMask.NameToLayer("Selectable");
    public static int terrain = 1 << LayerMask.NameToLayer("Terrain");
}

public static class Mouse
{

    public const int left = 0;
    public const int right = 1;
    public const int middle = 2;

    public static bool TryGetPosition(out Vector2 position, out RaycastHit hit) {
        position = default;
        Assert.IsTrue(Camera.main);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, float.MaxValue, Masks.terrain))
            return false;
        position = hit.point.ToVector2();
        return true;
    }
    public static bool TryGetPosition(out Vector2 position) {
        return TryGetPosition(out position, out _);
    }
}