using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CustomTrailRenderer))]
[ExecuteAlways]
public class TrackTrail : MonoBehaviour {
    public CustomTrailRenderer customTrailRenderer;
    public LayerMask layerMask;
    public float threshold = 0.1f;
    public float clearThreshold = 1;
    public int maxPoints = 64;
    public Queue<Vector3> points = new();
    public Queue<float> lengths = new();
    public Queue<float> times = new();
    public Vector3? head;
    public float offset = .01f;
    public float totalDistanceTraveled;

    public void Reset() {
        customTrailRenderer = GetComponent<CustomTrailRenderer>();
        Assert.IsTrue(customTrailRenderer);
    }
    public void OnEnable() {
        Clear();
    }
    public void Update() {
        if (head is not { } actualHead)
            TracePointAndAdd(0);
        else {
            var distance = Vector2.Distance(actualHead.ToVector2(), transform.position.ToVector2());
            if (distance > clearThreshold)
                Clear();
            else if (distance > threshold) {
                totalDistanceTraveled += distance;
                TracePointAndAdd(distance);
            }
        }
    }
    public void Enqueue(Vector3 point) {
        points.Enqueue(point);
        lengths.Enqueue(totalDistanceTraveled);
        times.Enqueue(Time.timeSinceLevelLoad);
        head = point;
    }
    public void Dequeue() {
        points.Dequeue();
        if (points.Count == 0)
            head = null;
        lengths.Dequeue();
        times.Dequeue();
    }
    public void Rebuild() {

        customTrailRenderer.points.Clear();
        customTrailRenderer.lengths.Clear();
        customTrailRenderer.times.Clear();

        customTrailRenderer.points.AddRange(points);
        customTrailRenderer.lengths.AddRange(lengths);
        customTrailRenderer.times.AddRange(times);

        customTrailRenderer.Rebuild();
    }
    public void Clear() {
        points.Clear();
        lengths.Clear();
        times.Clear();
        head = null;
        totalDistanceTraveled = 0;
        
        customTrailRenderer.points.Clear();
        customTrailRenderer.lengths.Clear();
        customTrailRenderer.Rebuild();
    }
    public void TracePointAndAdd(float distance) {
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