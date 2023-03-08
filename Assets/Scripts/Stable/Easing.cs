using System;
using UnityEngine;

public static class Easing {
    public static float InOutQuad(float t) =>  t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    public static float OutExpo(float t) => t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
}