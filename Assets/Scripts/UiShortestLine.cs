using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[RequireComponent(typeof(UiLineRenderer))]
public class UiShortestLine : MonoBehaviour {
    public UiLineRenderer lineRenderer;
    public Graphic a, b;
    private void Awake() {
        lineRenderer = GetComponent<UiLineRenderer>();
        Assert.IsTrue(lineRenderer);
    }
    private void Start() {
        lineRenderer.ClearPoints();
        lineRenderer.AddPoint(Vector2.zero);
        lineRenderer.AddPoint(Vector2.zero);
    }
    private void LateUpdate() {
        if (!a || !b || !a.isActiveAndEnabled || !b.isActiveAndEnabled ||
            !MathUtils.TryGetShortestLine(a.rectTransform.ToScreenSpace(), b.rectTransform.ToScreenSpace(), out var aa, out var bb)) {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.enabled = true;
        lineRenderer.rectTransform.anchorMax = lineRenderer.rectTransform.anchorMin = Vector2.zero;
        lineRenderer.rectTransform.pivot = Vector2.zero;
        lineRenderer[0] = aa;
        lineRenderer[1] = bb;
    }
}