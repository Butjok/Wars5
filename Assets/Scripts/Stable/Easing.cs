using System;
using UnityEngine;

public static class Easing {

    public enum Name {
        InOutQuad, OutExpo, Linear, OutSine
    }

    public static float InOutQuad(float t) => t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    public static float OutExpo(float t) => t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    public static float OutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2);

    public static float Dynamic(Name name, float t) {
        return name switch {
            Name.InOutQuad => InOutQuad(t),
            Name.OutExpo => OutExpo(t),
            Name.Linear => t,
            Name.OutSine => OutSine(t),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }
}