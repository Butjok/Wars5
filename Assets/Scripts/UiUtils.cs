using System.Collections.Generic;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public interface IUiBoundPoints {
    IEnumerable<Vector3> UiBoundPoints { get; }
}

public static class UiUtils {

    public static bool IsValid(this IUiBoundPoints uiBoundPoints) {
        return uiBoundPoints is Object o && o;
    }
    
    public static bool TryCalculateScreenBounds(Camera camera, IEnumerable<Vector3> points, out Vector3 min, out Vector3 max) {

        min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var count = 0;
        foreach (var point in points) {
            var screenPosition = camera.WorldToScreenPoint(point);
            min = Vector3.Min(min, screenPosition);
            max = Vector3.Max(max, screenPosition);
            count++;
        }
        Assert.AreNotEqual(0, count);

        return min.z > 0 && max.x > 0 && min.x < Screen.width && max.y > 0 && min.y < Screen.height;
    }

    public static bool TryEncapsulate(this RectTransform rectTransform, IEnumerable<Vector3> points, out float distance, Camera camera = null) {

        if (!camera)
            camera = Camera.main;
        Assert.IsTrue(camera);

        if (!TryCalculateScreenBounds(camera, points, out var min, out var max)) {
            distance = 0;
            return false;
        }

        var position = new Vector2(min.x, min.y);
        var size = new Vector2(max.x - min.x, max.y - min.y);

        rectTransform.anchorMin = rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        distance = (min.z + max.z) / 2;

        return true;
    }

    public static bool TryEncapsulate(this RectTransform rectTransform, IUiBoundPoints uiBoundPoints, out float distance, Camera camera = null) {

        if (uiBoundPoints.IsValid())
            return rectTransform.TryEncapsulate(uiBoundPoints.UiBoundPoints, out distance, camera);

        distance = 0;
        return false;
    }

    [Command] public static Vector2 alphaFading = new(15, 20);

    public static void FadeAlpha(this TMP_Text text, float distance) {
        if (!text.isActiveAndEnabled)
            return;
        var alpha = 1 - MathUtils.SmoothStep(alphaFading[0], alphaFading[1], distance);
        var color = text.color;
        color.a = alpha;
        text.color = color;
    }
    public static void FadeAlpha(this IEnumerable<TMP_Text> texts, float distance) {
        foreach (var text in texts)
            FadeAlpha(text, distance);
    }
}