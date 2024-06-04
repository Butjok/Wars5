using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CustomTrailRenderer))]
[ExecuteAlways]
public class TrackTrail : MonoBehaviour {
    public CustomTrailRenderer customTrailRenderer;
    public float threshold = 0.1f;
    public LayerMask layerMask;
    public int maxPoints = 64;
    public Queue<Vector3> points = new();
    public Queue<float> lengths = new();
    public Queue<float> times = new();
    public Vector3? head;
    public float lifetime = 2;
    public MaterialPropertyBlock materialPropertyBlock;
    public float offset = .01f;
    public float clearThreshold = 1;
    public float length;

    private List<Vector3> pointsList = new();
    private List<float> lengthsList = new();
    private List<float> timesList = new();

    public void Reset() {
        customTrailRenderer = GetComponent<CustomTrailRenderer>();
        Assert.IsTrue(customTrailRenderer);
    }
    public void OnEnable() {
        Clear();
    }
    public void Update() {
        if (head is not { } actualHead)
            Trace(0);
        else {
            var distance = Vector2.Distance(actualHead.ToVector2(), transform.position.ToVector2());
            if (distance > clearThreshold)
                Clear();
            else if (distance > threshold) 
                Trace(distance);
        }
    }
    public void Enqueue(Vector3 point) {
        points.Enqueue(point);
        lengths.Enqueue(length);
        times.Enqueue(Time.timeSinceLevelLoad);
        head = point;
    }
    public void Dequeue() {
        points.Dequeue();
        if (points.Count == 0)
            head = null;
        times.Dequeue();
        lengths.Dequeue();
    }
    public void Rebuild() {
        pointsList.Clear();
        lengthsList.Clear();
        timesList.Clear();
        pointsList.AddRange(points);
        lengthsList.AddRange(lengths);
        timesList.AddRange(times);
        customTrailRenderer.points = pointsList;
        customTrailRenderer.lengths = lengthsList;
        customTrailRenderer.times = timesList;
        customTrailRenderer.Rebuild();
    }
    public void Clear() {
        customTrailRenderer.points.Clear();
        customTrailRenderer.lengths.Clear();
        customTrailRenderer.Rebuild();
        points.Clear();
        times.Clear();
        head = null;
    }
    public void Trace(float distance) {
        var traceOrigin = transform.position + Vector3.up * 100;
        var traceDirection = Vector3.down;
        if (Physics.Raycast(new Ray(traceOrigin, traceDirection), out var hit, float.PositiveInfinity, layerMask)) {
            var position = hit.point + Vector3.up * offset;
            length += distance;
            Enqueue(position);
            while (points.Count > maxPoints)
                Dequeue();
            Rebuild();
        }
    }
}