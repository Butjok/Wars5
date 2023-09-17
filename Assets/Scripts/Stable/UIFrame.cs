using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class UIFrame : MonoBehaviour {

    public BoxCollider boxCollider;
    public RectTransform rectTransform;
    public IEnumerator jumpAnimation;

    public void Reset() {
        rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);
    }

    public void JumpTo(BoxCollider newTarget, float duration) {
        if (jumpAnimation != null)
            StopCoroutine(jumpAnimation);
        jumpAnimation = JumpAnimation(newTarget, duration);
        StartCoroutine(jumpAnimation);
    }

    public IEnumerator JumpAnimation(BoxCollider newTarget, float duration, Func<float,float> easing = null) {

        var oldTarget = boxCollider;
        boxCollider = newTarget;

        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            yield return null;

            if (!oldTarget || !newTarget ||
                !TryCalculateScreenSize(oldTarget, out var oldMin, out var oldMax) ||
                !TryCalculateScreenSize(newTarget, out var newMin, out var newMax))
                yield break;

            var t = (Time.time - startTime) / duration;
            t = (easing ?? Easing.InOutQuad)(t);
            var min = Vector2.Lerp(oldMin, newMin, t);
            var max = Vector2.Lerp(oldMax, newMax, t);

            rectTransform.anchoredPosition = min;
            rectTransform.sizeDelta = max - min;
        }

        jumpAnimation = null;
    }

    public static bool TryCalculateScreenSize(BoxCollider boxCollider, out Vector2 min, out Vector2 max) {

        min = max = default;

        var camera = Camera.main;
        if (!camera)
            return false;

        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);

        var size = boxCollider.size;
        for (var x = -1; x <= 1; x += 2)
        for (var y = -1; y <= 1; y += 2)
        for (var z = -1; z <= 1; z += 2) {

            var localPosition = boxCollider.center + new Vector3(x * size.x / 2, y * size.y / 2, z * size.z / 2);
            var worldPosition = boxCollider.transform.TransformPoint(localPosition);

            var screenPosition = camera.WorldToScreenPoint(worldPosition);

            if (screenPosition.z > 0) {

                min.x = Mathf.Min(min.x, screenPosition.x);
                min.y = Mathf.Min(min.y, screenPosition.y);

                max.x = Mathf.Max(max.x, screenPosition.x);
                max.y = Mathf.Max(max.y, screenPosition.y);
            }
            else
                return false;
        }
        return true;
    }

    public void LateUpdate() {

        if (jumpAnimation != null || !boxCollider || !TryCalculateScreenSize(boxCollider, out var min, out var max))
            return;

        rectTransform.anchoredPosition = min;
        rectTransform.sizeDelta = max - min;
    }
}