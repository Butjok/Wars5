using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Drawing;
using UnityEngine;

public static class DebugDraw {

    public enum ColorDirection {LightToDark, DarkToLight}
    
    public static void DistanceField(Dictionary<Vector2Int, float> distanceField, ColorDirection colorDirection = ColorDirection.DarkToLight) {

        var values = distanceField.Values.Where(d => d < global::DistanceField.infinity).ToList();
        if (values.Count == 0)
            return;

        float minDistance = values.Min();
        float maxDistance = values.Max();
        var range = maxDistance - minDistance;

        foreach (var (position, distance) in distanceField) {
            
            var isInfinite = distance >= global::DistanceField.infinity;
            var planeColor = isInfinite || range == 0 ? Color.black : distance.ToColor(minDistance, maxDistance, colorDirection == ColorDirection.DarkToLight);
            var labelColor = Color.black; //planeColor.YiqContrastColor();
            var label = distance < global::DistanceField.infinity ? distance.ToString("n2") : "inf";
            
            Draw.ingame.SolidPlane(position.ToVector3(), Vector3.up, Vector2.one, planeColor);
            Draw.ingame.Label2D(position.ToVector3(), label, 14, LabelAlignment.Center, labelColor);
        }
    }

    public static IEnumerator DrawUntilKey(KeyCode key, Action drawingAction) {
        while (true) {
            drawingAction();
            if (Input.GetKeyDown(key)) {
                yield return null;
                break;
            }
            yield return null;
        }
    }

    public static IEnumerator Sequence(IEnumerable<IEnumerator> items) {
        foreach (var item in items)
            yield return item;
    }
}