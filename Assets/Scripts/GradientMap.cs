using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GradientMap {

    [Serializable]
    public struct Point {
        public float position;
        public Color color;
    }

    public List<Point> points = new() {
        new() { position = 0, color = Color.red },
        new() { position = 1, color = Color.yellow }
    };

    public Color Sample(float position) {
        if (points.Count == 0)
            return Color.black;
        if (points.Count == 1)
            return points[0].color;

        position = Mathf.Clamp01(position);
        var rightIndex = 1;
        for (; rightIndex < points.Count; rightIndex++) {
            var leftIndex = rightIndex - 1;
            if (points[leftIndex].position <= position && points[rightIndex].position >= position)
                break;
        }

        if (rightIndex >= points.Count)
            return Color.black;

        var leftPosition = points[rightIndex - 1].position;
        var rightPosition = points[rightIndex].position;
        var range = rightPosition - leftPosition;

        var t = Mathf.Approximately(0, range) ? .5f : (position - leftPosition) / range;
        return Color.Lerp(points[rightIndex - 1].color, points[rightIndex].color, t);
    }
}