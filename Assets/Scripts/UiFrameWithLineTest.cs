using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class UiFrameWithLineTest : MonoBehaviour, IDragHandler {

    public Camera camera;
    public MeshFilter meshFilter;
    public RectTransform frameRectTransform;
    public UiLineRenderer lineRenderer;
    public RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);
    }

    private void Start() {
        lineRenderer.ClearPoints();
        lineRenderer.AddPoint(Vector2.zero);
        lineRenderer.AddPoint(Vector2.zero);
    }

    private void Update() {
        if (!meshFilter || !meshFilter.sharedMesh || !frameRectTransform)
            return;
        Assert.IsTrue(meshFilter.sharedMesh.vertexCount <= 50);

        var a = camera.GetScreenSpaceAABB(meshFilter.sharedMesh.vertices.Select(meshFilter.transform.TransformPoint));
        var b = new Rect(rectTransform.anchoredPosition, rectTransform.sizeDelta);

        frameRectTransform.anchoredPosition = a.min;
        frameRectTransform.sizeDelta = a.size;
        
        if (!MathUtils.TryGetShortestLine(a, b, out var aa, out var bb)) 
            lineRenderer.enabled = false;
        
        else {
            lineRenderer[0] = aa;
            lineRenderer[1] = bb;
            lineRenderer.enabled = true;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta;
    }
}