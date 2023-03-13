using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
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

    public List<Renderer> renderers = new();
    public string playerColorUniformName = "_PlayerColor";
    public string attackHighlightFactorUniformName = "_AttackHighlightFactor";
    public string attackHighlightStartTimeUniformName = "_AttackHighlightStartTime";
    public string movedUniformName = "_Moved";
    public TMP_Text hpText;
    public List<Wheel> wheels = new();
    public LayerMask terrainLayerMask;
    public Vector2 terrainBumpRange = new(0, .05f);
    public float terrainBumpTiling = 5;
    public Transform body;
    
    public Vector2 springLengthRange = new(.0f, .25f);
    public float springTargetLength = .125f;
    public float springForce = 100;
    public float springDrag = 3;

    public Vector3 bodyLocalSpaceTorquePosition;
    public Vector3 bodyLocalSpaceTorque;

    private void Reset() {
        renderers = GetComponentsInChildren<Renderer>().ToList();
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
        set => transform.position = value.ToVector3Int();
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

    private void UpdateSpring(Wheel wheel) {

        var length = Vector3.Dot(body.up, wheel.springWeightPosition - wheel.position);
        var force = (springTargetLength - length) * springForce;
        force -= wheel.springVelocity * springDrag;
        
        // TODO: apply body torque
        
        wheel.springVelocity += force * Time.fixedDeltaTime;
        if (float.IsNaN(wheel.springVelocity))
            wheel.springVelocity = 0;

        length += wheel.springVelocity * Time.fixedDeltaTime;
        length = Mathf.Clamp(length, springLengthRange[0], springLengthRange[1]);

        wheel.springWeightPosition = wheel.position + body.up * length;
    }

    private void UpdateBodyPosition() {
        if (wheels.Count > 0) {
            var sum = Vector3.zero;
            foreach (var wheel in wheels)
                sum += wheel.springWeightPosition;
            body.position = sum / wheels.Count;
        }
    }

    private float lastSpringUpdateTime = float.MinValue;
    private void UpdateSprings() {
        if (lastSpringUpdateTime == Time.unscaledTime)
            return;
        lastSpringUpdateTime = Time.unscaledTime;
        foreach (var wheel in wheels)
            UpdateSpring(wheel);
    }


    private void OnEnable() {
        UpdateAxes();
    }

    private void FixedUpdate() {
        UpdateSprings();
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

        UpdateSprings();
        UpdateBodyRotation();
        UpdateBodyPosition();
    }
}