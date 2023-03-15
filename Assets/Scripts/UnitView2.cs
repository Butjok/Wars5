using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using static UnitView2.Wheel.Axis.Constants;

[ExecuteInEditMode]
public class UnitView2 : MonoBehaviour {

    [Serializable]
    public class Wheel {

        public class Axis {

            public static class Constants {
                public const int left = 0;
                public const int right = 1;
                public const int front = 1;
                public const int back = 0;
            }

            public Wheel left, right;
            public Wheel this[int side] {
                get => side == Constants.left ? left : right;
                set {
                    if (side == Constants.left)
                        left = value;
                    else
                        right = value;
                }
            }
        }

        public const float rayOriginHeight = 1000;

        public Transform transform;
        public Vector2 projectedOriginPosition;
        public float radius;
        public Vector2 steeringRange = new();
        public float steeringResponsiveness = 1;

        public int Side => projectedOriginPosition.x < 0 ? left : right;

        [NonSerialized] public float spinAngle;
        [NonSerialized] public Vector3? previousPosition;
        [NonSerialized] public Vector3 position;
        [NonSerialized] public Quaternion rotation;
        [NonSerialized] public Vector3 springWeightPosition;
        [NonSerialized] public float springVelocity;
        [NonSerialized] public float steeringAngle;
    }

    [Header("Shading")]
    public List<Renderer> renderers = new();
    public string playerColorUniformName = "_PlayerColor";
    public string attackHighlightFactorUniformName = "_AttackHighlightFactor";
    public string attackHighlightStartTimeUniformName = "_AttackHighlightStartTime";
    public string movedUniformName = "_Moved";

    [Header("UI")]
    public TMP_Text hpText;

    [Header("Parts")]
    public Transform body;
    public List<Wheel> wheels = new();

    [Header("Terrain")]
    public LayerMask terrainLayerMask;
    public Vector2 terrainBumpRange = new(0, .05f);
    public float terrainBumpTiling = 5;

    [Header("Springs")]
    public Vector2 springLengthRange = new(.0f, .25f);
    public float springTargetLength = .125f;
    public float springForce = 100;
    public float springDrag = 3;
    public Vector3 bodyCenterOfMass;

    [Header("Acceleration")]
    public float accelerationTorqueMultiplier = 100;
    public double accelerationCalculationTimeRange = 1;
    public bool drawAccelerationGraph = false;
    public float graphHeight = 300;
    public float graphYHalfRange = 5;

    [Header("Battle View")]
    public List<Transform> hitPoints = new();
    public int incomingProjectilesCount = 0;

    private void Reset() {
        renderers = GetComponentsInChildren<Renderer>().ToList();
    }

    [ContextMenu(nameof(FindHitPoints))]
    private void FindHitPoints() {
        hitPoints.Clear();
        hitPoints.AddRange(GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("HitPoint")));
    }

    private MaterialPropertyBlock materialPropertyBlock;
    private MaterialPropertyBlock MaterialPropertyBlock => materialPropertyBlock ??= new MaterialPropertyBlock();
    private void ApplyMaterialPropertyBlock() {
        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    private Color? playerColor;
    [Command]
    public Color PlayerColor {
        get {
            if (playerColor is { } actualValue)
                return actualValue;
            throw new AssertionException("playerColor == null", null);
        }
        set {
            playerColor = value;
            MaterialPropertyBlock.SetColor(playerColorUniformName, value);
            ApplyMaterialPropertyBlock();
        }
    }

    [Command]
    public Vector2Int Position {
        get => transform.position.ToVector2().RoundToInt();
        set {
            transform.position = value.ToVector3Int();
            ResetSprings();
            ResetSteering();
        }
    }
    public Func<bool> MoveAlong(IEnumerable<Vector2Int> path, Vector2Int? finalDirection = null) {
        var completed = false;
        StartCoroutine(new MoveSequence(transform, path, _finalDirection: finalDirection, onComplete: () => completed = true).Animation());
        return () => completed;
    }

    [Command]
    public Vector2Int LookDirection {
        get {
            var result = transform.forward.ToVector2().RoundToInt();
            Assert.AreEqual(1, result.ManhattanLength());
            return result;
        }
        set {
            Assert.AreEqual(1, value.ManhattanLength());
            transform.rotation = Quaternion.LookRotation(value.ToVector3Int(), Vector3.up);
            ResetSprings();
            ResetSteering();
        }
    }
    [Command]
    public void RotateTowards(Vector2Int value) {
        MoveAlong(null, value);
    }

    [Command]
    public bool Visible {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    private int? hp;
    [Command]
    public int Hp {
        get {
            if (hp is { } actualValue)
                return actualValue;
            throw new AssertionException("hp == null", null);
        }
        set {
            hp = value;
            if (hpText)
                hpText.text = value.ToString();
        }
    }
    [Command]
    public void AnimateHp(int value) {
        if (hpText)
            StartCoroutine(HpAnimation(value));
    }
    private IEnumerator HpAnimation(int to) {
        if (hp == to)
            yield break;
        var from = hp ?? 0;
        var step = to > from ? 1 : -1;
        for (var i = @from;; i += step) {
            Hp = i;
            if (i == to)
                break;
            yield return null;
        }
    }

    private bool? highlightAsTarget;
    [Command]
    public bool HighlightAsTarget {
        get {
            if (highlightAsTarget is { } actualValue)
                return actualValue;
            throw new AssertionException("highlightAsTarget == null", null);
        }
        set {
            highlightAsTarget = value;
            MaterialPropertyBlock.SetFloat(attackHighlightFactorUniformName, value ? 1 : 0);
            MaterialPropertyBlock.SetFloat(attackHighlightStartTimeUniformName, Time.timeSinceLevelLoad);
            ApplyMaterialPropertyBlock();
        }
    }

    private bool? moved;
    [Command]
    public bool Moved {
        get {
            if (moved is { } actualValue)
                return actualValue;
            throw new AssertionException("moved == null", null);
        }
        set {
            moved = value;
            MaterialPropertyBlock.SetFloat(movedUniformName, value ? 1 : 0);
            ApplyMaterialPropertyBlock();
        }
    }

    private void UpdateWheel(Wheel wheel) {

        // TODO: do steering of the wheels

        var spinRotation = Quaternion.Euler(wheel.spinAngle, 0, 0);
        var steeringRotation = Quaternion.Euler(0, wheel.steeringAngle, 0);
        wheel.rotation = body.rotation * steeringRotation * spinRotation;

        var projectedOriginLocalPosition = new Vector3(wheel.projectedOriginPosition.x, 0, wheel.projectedOriginPosition.y);
        var projectedOriginWorldPosition = transform.TransformPoint(projectedOriginLocalPosition);
        var originWorldPosition = projectedOriginWorldPosition + Wheel.rayOriginHeight * Vector3.up;
        var ray = new Ray(originWorldPosition, Vector3.down);

        var hasHit = Physics.SphereCast(ray, wheel.radius, out var hit, float.MaxValue, terrainLayerMask);
        if (hasHit) {
            wheel.position = ray.GetPoint(hit.distance);

            var noise = Mathf.PerlinNoise(wheel.position.x * terrainBumpTiling, wheel.position.z * terrainBumpTiling);
            var height = Mathf.Lerp(terrainBumpRange[0], terrainBumpRange[1], noise);
            wheel.position += height * Vector3.up;
        }
        else {
            wheel.position = originWorldPosition;
        }

        if (wheel.previousPosition is { } actualPreviousPosition) {
            var wheelForward = body.rotation * steeringRotation * Vector3.forward;
            var deltaPosition = wheel.position - actualPreviousPosition;
            var distance = Vector3.Dot(deltaPosition, wheelForward);
            const float ratio = 180 / Mathf.PI;
            var deltaAngle = distance / wheel.radius * ratio;
            wheel.spinAngle += deltaAngle;
        }

        wheel.previousPosition = wheel.position;
    }

    private Dictionary<float, Wheel.Axis> axes = new();
    private List<Wheel.Axis> pitchAxes = new();

    [ContextMenu(nameof(UpdateAxes))]
    private void UpdateAxes() {

        axes.Clear();
        foreach (var wheel in wheels) {
            if (!axes.TryGetValue(wheel.projectedOriginPosition.y, out var axis))
                axis = axes[wheel.projectedOriginPosition.y] = new Wheel.Axis();
            axis[wheel.Side] = wheel;
        }

        pitchAxes.Clear();
        var ys = axes.Keys.OrderBy(y => y).ToList();
        for (var i = 1; i < ys.Count; i++) {
            var frontAxis = axes[ys[i]];
            var backAxis = axes[ys[i - 1]];
            for (var side = left; side <= right; side++)
                pitchAxes.Add(new Wheel.Axis {
                    [back] = backAxis[side],
                    [front] = frontAxis[side]
                });
        }
    }

    private void UpdateBodyRotation() {

        if (!Application.isPlaying)
            UpdateAxes();

        if (axes.Count > 0 && pitchAxes.Count > 0) {

            var rightSum = Vector3.zero;
            foreach (var axis in axes.Values)
                rightSum += axis[right].springWeightPosition - axis[left].springWeightPosition;
            rightSum /= axes.Count;

            var forwardSum = Vector3.zero;
            foreach (var axis in pitchAxes)
                forwardSum += axis[front].springWeightPosition - axis[back].springWeightPosition;
            forwardSum /= pitchAxes.Count;

            var up = Vector3.Cross(rightSum, forwardSum);
            if (Vector3.Dot(Vector3.up, up) < 0)
                up = -up;

            body.rotation = Quaternion.LookRotation(forwardSum, up);
        }
    }

    private List<Vector3> torques = new();
    private float? lastSpringUpdateTime;
    private void UpdateSpring(Wheel wheel, Vector3 accelerationTorque) {

        if (lastSpringUpdateTime is not { } lastTime)
            throw new AssertionException(null, null);
        var deltaTime = Time.time - lastTime;

        var springLength = Vector3.Dot(body.up, wheel.springWeightPosition - wheel.position);
        var springForce = (springTargetLength - springLength) * this.springForce;
        springForce -= wheel.springVelocity * springDrag;

        var centerOfMass = body.TransformPoint(bodyCenterOfMass);
        void AddTorque(Vector3 torque) {
            springForce += Vector3.Dot(Vector3.Cross(torque, wheel.position - centerOfMass), body.up);
        }

        AddTorque(accelerationTorque);
        foreach (var torque in torques)
            AddTorque(torque);

        wheel.springVelocity += springForce * deltaTime;
        if (float.IsNaN(wheel.springVelocity))
            wheel.springVelocity = 0;

        springLength += wheel.springVelocity * deltaTime;
        springLength = Mathf.Clamp(springLength, springLengthRange[0], springLengthRange[1]);

        wheel.springWeightPosition = wheel.position + body.up * springLength;
    }

    private void UpdateBodyPosition() {
        if (wheels.Count > 0) {
            var sum = Vector3.zero;
            foreach (var wheel in wheels)
                sum += wheel.springWeightPosition;
            body.position = sum / wheels.Count;
        }
    }

    private void UpdateSprings(bool isCalledFromUpdate) {
        if (lastSpringUpdateTime != null) {
            var accelerationTorque = TryCalculateAcceleration(isCalledFromUpdate, out _, out var acceleration) ? body.right * acceleration * accelerationTorqueMultiplier : Vector3.zero;
            foreach (var wheel in wheels)
                UpdateSpring(wheel, accelerationTorque);
        }
        lastSpringUpdateTime = Time.time;
    }
    
    private List<(double time, double position)> accelerationPoints = new();
    private bool TryCalculateAcceleration(bool canDrawGraph, out float speed, out float acceleration) {

        double GetXInGraphSpace(double time) {
            var deltaTime = Time.timeAsDouble - time;
            return 1 - deltaTime / accelerationCalculationTimeRange;
        }
        double GetYInGraphSpace(double position) {
            return position / graphYHalfRange;
        }
        double GetXInScreenSpace(double xInGraphSpace) {
            return Screen.width * xInGraphSpace;
        }
        double GetYInScreenSpace(double yInGraphSpace) {
            return graphHeight / 2 + (yInGraphSpace / graphYHalfRange) * Screen.height;
        }
        Vector3 GetScreenPosition((double time, double position) point) {
            return new Vector3(
                (float)GetXInScreenSpace(GetXInGraphSpace(point.time)),
                (float)GetYInScreenSpace(GetYInGraphSpace(point.position)));
        }

        using (canDrawGraph && drawAccelerationGraph ? (IDisposable)Draw.ingame.InScreenSpace(Camera.main) : new CommandBuilder.ScopeEmpty())
        using (canDrawGraph && drawAccelerationGraph ? (IDisposable)Draw.ingame.WithLineWidth(1.5f) : new CommandBuilder.ScopeEmpty()) {

            speed = acceleration = 0;
            
            if (positions.Count <= 0)
                return false;

            var points = accelerationPoints;
            points.Clear();
            points.Add((positions[0].time, 0));
                
            float position = 0;
            for (var i = 1; i < positions.Count; i++) {
                var previous = positions[i - 1];
                var next = positions[i];
                var forward = previous.forward;
                var delta = next.position - previous.position;
                var offset = Vector3.Dot(forward, delta);
                position += offset;
                points.Add((next.time, position));
            }

            if (drawAccelerationGraph)
                foreach (var point in points) {
                    Draw.ingame.SolidCircleXY(GetScreenPosition(point), 5, Color.white);
                }

            if (points.Count < 3)
                return false;
            
            var p0 = points[0];
            var p1 = points[points.Count / 2];
            var p2 = points[^1];

            // http://www2.lawrence.edu/fast/GREGGJ/CMSC210/arithmetic/interpolation.html
            var diff10 = (p1.position - p0.position) / (p1.time - p0.time);
            var diff21 = (p2.position - p1.position) / (p2.time - p1.time);
            var c = p0.position;
            var b = diff10;
            var a = (diff21 - diff10) / (p2.time - p0.time);

            var a1 = a;
            var b1 = -a * p0.time - a * p1.time + b;
            var c1 = a * p0.time * p1.time - b * p0.time + c;

            if (drawAccelerationGraph)
                for (var x = p0.time; x <= p2.time; x += .005) {
                    // var y = a * (x - p1.time) * (x - p0.time) + b * (x - p0.time) + c;
                    var y1 = a1 * x * x + b1 * x + c1;

                    // Draw.ingame.SolidCircleXY(GetScreenPosition((x, y)), 1, Color.cyan);
                    Draw.ingame.SolidCircleXY(GetScreenPosition((x, y1)), 1, Color.yellow);
                }

            speed = (float)(2 * a1 * p2.time + b1);
            acceleration = (float)(2 * a1);

            if (drawAccelerationGraph) {
                Draw.ingame.Label2D(new Vector3(50, 50), $"speed: {speed:0.##}\nacceleration: {acceleration:0.##}");
                Draw.ingame.SolidCircleXY(GetScreenPosition((p2.time - accelerationCalculationTimeRange / 2, acceleration)), 10, Color.red);
            }

            return true;
        }
    }

    private List<(double time, Vector3 position, Vector3 forward)> positions = new();
    private void RecordPosition() {
        while (positions.Count > 0 && positions[0].time < Time.timeAsDouble - accelerationCalculationTimeRange)
            positions.RemoveAt(0);
        positions.Add((Time.timeAsDouble, transform.position, transform.forward));
    }

    public void ResetSprings() {
        foreach (var wheel in wheels) {
            wheel.springVelocity = 0;
            wheel.springWeightPosition = wheel.position + body.up * springTargetLength;
        }
        torques.Clear();
    }

    public void ResetSteering() {
        foreach (var wheel in wheels)
            wheel.steeringAngle = 0;
    }

    public void ApplyImpactTorque(Vector3 position, Vector3 force, bool debug = false) {

        var centerOfMass = body.TransformPoint(bodyCenterOfMass);
        var torque = Vector3.Cross(position - centerOfMass, force);
        StartCoroutine(InstantTorque(torque));

        if (debug)
            using (Draw.WithDuration(3))
                Draw.Arrow(position, position + force);
    }
    [Command]
    public void ApplyImpactTorque(Vector3 localPosition, Vector3 localForce) {
        ApplyImpactTorque(transform.TransformPoint(localPosition), transform.TransformPoint(localForce), true);
    }
    private IEnumerator InstantTorque(Vector3 torque) {
        torques.Add(torque);
        yield return null;
        torques.Remove(torque);
    }

    private void OnEnable() {
        ResetSprings();
        ResetSteering();
        UpdateAxes();
    }

    private void FixedUpdate() {
        UpdateSprings(false);
    }

    private void Update() {

        foreach (var wheel in wheels) {

            UpdateWheel(wheel);

            if (wheel.transform)
                wheel.transform.SetPositionAndRotation(wheel.position, wheel.rotation);

            var matrix = Matrix4x4.TRS(wheel.position, wheel.rotation, Vector3.one);
            using (Draw.ingame.WithMatrix(matrix))
                Draw.ingame.WireSphere(0, wheel.radius);
        }

        RecordPosition();
        UpdateSprings(true);
        UpdateBodyRotation();
        UpdateBodyPosition();
    }
}