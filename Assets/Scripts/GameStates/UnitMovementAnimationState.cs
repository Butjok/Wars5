using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitMovementAnimationState {

    public static IEnumerator Run(Main main, Unit unit, IReadOnlyList<Vector2Int> path) {
        
        var initialLookDirection = unit.view.LookDirection;
        var animation = new Path(unit.view.transform, path, main.settings.unitSpeed).Animation();
        
        while (animation.MoveNext()) {
            yield return null;
            
            if (Input.GetMouseButtonDown(Mouse.left) || Input.GetMouseButtonDown(Mouse.right)) {
                unit.view.Position = path[^1];
                if (path.Count >= 2)
                    unit.view.LookDirection = path[^1] - path[^2];
                break;
            }
        }
        
        yield return ActionSelectionState.Run(main, unit, path, initialLookDirection);
    }
}