using System;
using UnityEngine;
using UnityEngine.UIElements;

public static class Easing {

    public enum Name {
        InOutQuad, OutExpo, Linear, OutSine, OutQuad, OutCubic, OutCirc, InOutCirc, InQuad
    }

    public static float InOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    public static float OutExpo(float t) => t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    public static float OutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2);
    public static float OutQuad(float t) => 1 - (1 - t) * (1 - t);
    public static float OutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
    public static float OutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
    public static float InOutCirc(float t) => t < 0.5f
        ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2
        : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;
    public static float InQuad(float t) => t * t;

    public static float Dynamic(Name name, float t) {
        return name switch {
            Name.InOutQuad => InOutQuad(t),
            Name.OutExpo => OutExpo(t),
            Name.Linear => t,
            Name.OutSine => OutSine(t),
            Name.OutQuad => OutQuad(t),
            Name.OutCubic => OutCubic(t),
            Name.OutCirc => OutCirc(t),
            Name.InOutCirc => InOutCirc(t),
            Name.InQuad=>InQuad(t),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }
}