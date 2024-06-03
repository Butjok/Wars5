using System.Collections.Generic;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class TrackTrail : MonoBehaviour {
    public LineRenderer lineRenderer;
    public float threshold = 0.1f;
    public LayerMask layerMask;
    public int maxPoints = 64;
    public Queue<Vector3> points = new();
    public Queue<float> creationTimes = new();
    public Vector3? head;
    public float lifetime = 2;
    public MaterialPropertyBlock materialPropertyBlock;
    public float offset = .01f;
    public float clearThreshold = 1;
    public void Reset() {
        lineRenderer = GetComponent<LineRenderer>();
        Assert.IsTrue(lineRenderer);
    }
    public void OnEnable() {
        Clear();
    }
    public void Update() {
        if (head is not {} actualHead)
            Trace();
        else {
            var distance = Vector2.Distance(actualHead.ToVector2(), transform.position.ToVector2());
            if (distance > clearThreshold)
                Clear();
            else if (distance > threshold)
                Trace();
        }
        var shouldRebuild = false;
        /*while (points.Count > 0 && Time.time - creationTimes.Peek() > lifetime) {
            Dequeue();
            shouldRebuild = true;
        }*/
        if (shouldRebuild)
            Rebuild();
    }
    public void Enqueue(Vector3 point) {
        points.Enqueue(point);
        creationTimes.Enqueue(Time.time);
        head = point;
    }
    public void Dequeue() {
        points.Dequeue();
        if (points.Count == 0)
            head = null;
        creationTimes.Dequeue();
    }
    public void Rebuild() {
        lineRenderer.positionCount = points.Count;
        var index = 0;
        foreach (var point in points) {
            lineRenderer.SetPosition(index, point);
            index++;
        }
        var length = 0f;
        for (var i = 0; i < lineRenderer.positionCount - 1; i++)
            length += Vector3.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i + 1));
        materialPropertyBlock ??= new MaterialPropertyBlock();
        materialPropertyBlock.SetFloat("_Length", length);
        lineRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void Clear() {
        lineRenderer.positionCount = 0;
        points.Clear();
        creationTimes.Clear();
        head = null;
    }
    public void Trace() {
        var traceOrigin = transform.position + Vector3.up * 100;
        var traceDirection = Vector3.down;
        if (Physics.Raycast(new Ray(traceOrigin, traceDirection), out var hit, float.PositiveInfinity, layerMask)) {
            var position = hit.point + Vector3.up * offset;
            Enqueue(position);
            while (points.Count > maxPoints)
                Dequeue();
            Rebuild();
        }
    }
}