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

    [Serializable]
    public class Turret {

        public string name = "Turret";
        public bool rotate = true;
        public bool ignoreTarget = false;
        public Transform transform;
        [SerializeField] private Vector3 position;
        public float angle;
        public List<Barrel> barrels = new();
        [NonSerialized] public float velocity;

        public Vector3 Position => transform ? transform.localPosition : position;

        [Serializable]
        public class Barrel {

            public string name = "MainGun";
            public bool rotate = true;
            public bool ignoreTarget = false;
            public Transform transform;
            [SerializeField] private Vector3 position;
            public float angle;
            public Vector2 clamp = new(-45, 15);
            [NonSerialized] public float velocity;

            public Vector3 Position => transform ? transform.localPosition : position;
        }
    }

    public Transform body;
    public Vector3 target;
    public bool drawTurretNames = false;
    public bool drawBarrelNames = false;
    public Transform debugTarget;

    private const string headerPrefix = "";
    private const string headerSeparator = " / ";

    [Space]
    [Header(headerPrefix + "TURRETS")]
    public List<Turret> turrets = new();

    [Space]
    [Header(headerPrefix + "WHEELS")]
    public LayerMask terrainLayerMask;
    public Vector2 terrainBumpRange = new(0, .05f);
    public float terrainBumpTiling = 5;
    public List<Wheel> wheels = new();

    [Space]
    [Header(headerPrefix + "PHYSICS" + headerSeparator + "SUSPENSION")]
    public Vector2 springLengthRange = new(.0f, .25f);
    public float springTargetLength = .125f;
    public float springForce = 250;
    public float springDrag = 8;
    public Vector3 bodyCenterOfMass;
    public float maxSpringVelocity = 5;

    [Space]
    [Header(headerPrefix + "PHYSICS" + headerSeparator + "ACCELERATION")]
    public float accelerationTorqueMultiplier = -5;
    public double accelerationCalculationTimeRange = .25f;
    public bool drawAccelerationGraph = false;
    public float graphHeight = 300;
    public float graphYHalfRange = 6;

    [Space]
    [Header(headerPrefix + "PHYSICS" + headerSeparator + "TURRETS")]
    public float turretSpringForce = 1000;
    public float turretSpringDrag = 10;
    public float turretMaxVelocity = 270;

    [Space]
    [Header(headerPrefix + "PHYSICS" + headerSeparator + "BARRELS")]
    public float barrelSpringForce = 1000;
    public float barrelSpringDrag = 20;
    public float barrelMaxVelocity = 180;
    public float barrelShotForce = 25;

    [Space]
    [Header(headerPrefix + "UI")]
    public TMP_Text hpText;
    public Vector3 uiBoxCenter;
    public Vector3 uiBoxHalfSize = new(.5f, .5f, .5f);
    public float guiClippingDistance = .5f;
    public Vector2 hpTextAlphaFading = new(10, 20);
    public bool drawUiBounds = false;

    [Space]
    [Header(headerPrefix + "SHADING")]
    public List<Renderer> renderers = new();
    public string playerColorUniformName = "_PlayerColor";
    public string attackHighlightFactorUniformName = "_AttackHighlightFactor";
    public string attackHighlightStartTimeUniformName = "_AttackHighlightStartTime";
    public string movedUniformName = "_Moved";

    [Space]
    [Header(headerPrefix + "BATTLE VIEW")]
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
            PlaceOnTerrain();
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
            PlaceOnTerrain();
        }
    }
    [Command]
    public void RotateTowards(Vector2Int value) {
        MoveAlong(null, value);
    }

    public void PlaceOnTerrain() {
        foreach (var wheel in wheels)
            UpdateWheel(wheel);
        ResetSprings();
        ResetSteering();
    }

    [Command]
    public bool Visible {
        get => gameObject.activeSelf;
        set {
            gameObject.SetActive(value);
            if (value)
                PlaceOnTerrain();
        }
    }

    public int? maxHp = 10;
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
            if (value != maxHp && hpText)
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
        else
            wheel.position = projectedOriginWorldPosition;

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

    private Queue<Vector3> instantaneousTorques = new();
    private void UpdateSpring(Wheel wheel, Vector3 accelerationTorque, float deltaTime) {

        var springLength = Vector3.Dot(body.up, wheel.springWeightPosition - wheel.position);
        var springForce = (springTargetLength - springLength) * this.springForce;
        springForce -= wheel.springVelocity * springDrag;

        var centerOfMass = body.position + body.rotation * bodyCenterOfMass;
        void AddTorque(Vector3 torque) {
            // Draw.ingame.Arrow(centerOfMass, centerOfMass + torque);
            var force = Vector3.Dot(Vector3.Cross(torque, wheel.position - centerOfMass), body.up);
            springForce += force;
            // Draw.ingame.Arrow(wheel.position, wheel.position + body.up * force);
        }

        AddTorque(accelerationTorque);
        foreach (var torque in instantaneousTorques)
            AddTorque(torque / deltaTime);

        wheel.springVelocity += springForce * deltaTime;
        if (float.IsNaN(wheel.springVelocity))
            wheel.springVelocity = 0;
        if (wheel.springVelocity > maxSpringVelocity)
            wheel.springVelocity = maxSpringVelocity;
        if (wheel.springVelocity < -maxSpringVelocity)
            wheel.springVelocity = -maxSpringVelocity;

        springLength += wheel.springVelocity * deltaTime;
        springLength = Mathf.Clamp(springLength, springLengthRange[0], springLengthRange[1]);

        wheel.springWeightPosition = wheel.position + body.up * springLength;
    }

    private void UpdateBodyPosition() {
        if (wheels.Count > 0) {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var wheel in wheels) {
                min = Vector3.Min(min, wheel.springWeightPosition);
                max = Vector3.Max(max, wheel.springWeightPosition);
            }
            body.position = Vector3.Lerp(min, max, .5f);
        }
    }

    private float? lastSpringUpdateTime;
    private void UpdateSprings(bool isCalledFromUpdate) {
        if (lastSpringUpdateTime is { } actualLastTime) {
            var accelerationTorque = TryCalculateAcceleration(isCalledFromUpdate && drawAccelerationGraph, out _, out var acceleration)
                ? body.right * (float)(acceleration * accelerationTorqueMultiplier)
                : Vector3.zero;
            foreach (var wheel in wheels)
                UpdateSpring(wheel, accelerationTorque, Time.time - actualLastTime);
            instantaneousTorques.Clear();
        }
        lastSpringUpdateTime = Time.time;
    }

    private List<(double time, double position)> accelerationPoints = new();
    private bool TryCalculateAcceleration(bool canDrawGraph, out double speed, out double acceleration) {

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

        using (canDrawGraph ? (IDisposable)Draw.ingame.InScreenSpace(Camera.main) : new CommandBuilder.ScopeEmpty())
        using (canDrawGraph ? (IDisposable)Draw.ingame.WithLineWidth(1.5f) : new CommandBuilder.ScopeEmpty()) {

            speed = acceleration = 0;

            if (positions.Count <= 0)
                return false;

            var points = accelerationPoints;
            points.Clear();
            float position = 0;
            points.Add((positions[0].time, position));

            for (var i = 1; i < positions.Count; i++) {
                var previous = positions[i - 1];
                var next = positions[i];
                var forward = previous.forward;
                var delta = next.position - previous.position;
                var offset = Vector3.Dot(forward, delta);
                position += offset;
                points.Add((next.time, position));
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

            speed = (2 * a1 * p2.time + b1);
            acceleration = (2 * a1);

            if (canDrawGraph) {
                foreach (var point in points)
                    Draw.ingame.SolidCircleXY(GetScreenPosition(point), 5, Color.white);

                for (var time = p0.time; time <= p2.time; time += (p2.time - p0.time) / 10)
                    Draw.ingame.SolidCircleXY(GetScreenPosition((time, pos: a1 * time * time + b1 * time + c1)), 1, Color.cyan);

                Draw.ingame.Label2D(new Vector3(50, 50), $"speed: {speed:0.##}\nacceleration: {acceleration:0.##}");
                Draw.ingame.SolidCircleXY(GetScreenPosition((p2.time - accelerationCalculationTimeRange / 2, acceleration)), 10, Color.red);
            }

            return true;
        }
    }

    private List<(double time, Vector3 position, Vector3 forward)> positions = new();
    private void RecordPosition() {
        positions.RemoveAll(item => item.time < Time.timeAsDouble - accelerationCalculationTimeRange);
        positions.Add((Time.timeAsDouble, transform.position, transform.forward));
    }

    public void ResetSprings() {
        foreach (var wheel in wheels) {
            wheel.springVelocity = 0;
            wheel.springWeightPosition = wheel.position + body.up * springTargetLength;
        }
        instantaneousTorques.Clear();
        positions.Clear();
    }

    public void ResetSteering() {
        foreach (var wheel in wheels)
            wheel.steeringAngle = 0;
    }

    public void ApplyInstantaneousTorque(Vector3 position, Vector3 force, bool debug = false) {

        var centerOfMass = body.position + body.rotation * bodyCenterOfMass;
        var torque = Vector3.Cross(position - centerOfMass, force);
        instantaneousTorques.Enqueue(torque);
        //
        // if (debug)
        //     using (Draw.WithDuration(3))
        //         Draw.Arrow(position, position + force);
    }
    [Command]
    public void ApplyInstantaneousLocalForce(Vector3 localPosition, Vector3 localForce) {
        ApplyInstantaneousTorque(transform.TransformPoint(localPosition), transform.TransformPoint(localForce), true);
    }
    public void ApplyInstantaneousWorldForce(Vector3 position, Vector3 force) {
        ApplyInstantaneousTorque(position, force, true);
    }
    public void ApplyInstantaneousBarrelShotForce(Turret turret, Turret.Barrel barrel) {

        var turretRotation = Quaternion.Euler(0, turret.angle, 0);
        var barrelPosition = body.position + body.rotation * (turret.Position + turretRotation * barrel.Position);
        var barrelRotation = Quaternion.Euler(barrel.angle, 0, 0);

        var barrelBack = body.rotation * turretRotation * barrelRotation * Vector3.back;
        using (Draw.ingame.WithDuration(2))
            Draw.ingame.Arrow(barrelPosition, barrelPosition + barrelBack, Color.yellow);

        ApplyInstantaneousWorldForce(barrelPosition, barrelBack * barrelShotForce);
    }
    [Command]
    public void ApplyInstantaneousBarrelShotForce(string turretName, string barrelName) {
        foreach (var turret in turrets.Where(t => t.name.StartsWith(turretName)))
        foreach (var barrel in turret.barrels.Where(b => b.name.StartsWith(barrelName)))
            ApplyInstantaneousBarrelShotForce(turret, barrel);
    }

    private void OnEnable() {
        UpdateAxes();
        PlaceOnTerrain();
    }

    private void UpdateHpTextPosition() {

        var camera = Camera.main;
        if (!camera)
            return;

        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (var x = -1; x <= 1; x += 2)
        for (var y = -1; y <= 1; y += 2)
        for (var z = -1; z <= 1; z += 2) {
            var localPosition = uiBoxHalfSize;
            localPosition.Scale(new Vector3(x, y, z));
            localPosition += uiBoxCenter;
            var worldPosition = body.position + body.rotation * localPosition;
            if (drawUiBounds)
                Draw.ingame.Cross(worldPosition, .1f);
            var screenPosition = camera.WorldToScreenPoint(worldPosition);
            min = Vector3.Min(min, screenPosition);
            max = Vector3.Max(max, screenPosition);
        }

        var isInFront = min.z > guiClippingDistance;
        var shouldBeVisible = hp != maxHp && isInFront;
        if (hpText && hpText.enabled != shouldBeVisible)
            hpText.enabled = shouldBeVisible;
        if (hpText.enabled) {
            hpText.rectTransform.anchoredPosition = new Vector2(min.x, min.y);
            hpText.rectTransform.sizeDelta = new Vector2(max.x - min.x, max.y - min.y);
            var color = hpText.color;
            color.a = 1 - MathUtils.SmoothStep(hpTextAlphaFading[0], hpTextAlphaFading[1], (min.z + max.z) / 2);
            hpText.color = color;
        }
    }
    
    private void FixedUpdate() {

        UpdateSprings(false);

        if (debugTarget)
            target = debugTarget.position;

        foreach (var turret in turrets) {

            if (!turret.rotate)
                continue;

            var turretPosition = body.position + body.rotation * turret.Position;
            var turretPlane = new Plane(body.up, turretPosition);
            var targetOnTurretPlane = turretPlane.ClosestPointOnPlane(target);
            var turretTo = targetOnTurretPlane - turretPosition;
            var turretRotation = Quaternion.Euler(0, turret.angle, 0);
            var turretFrom = body.rotation * turretRotation * Vector3.forward;
            var turretDeltaAngle = !turret.ignoreTarget ? Vector3.SignedAngle(turretFrom, turretTo, turretPlane.normal) : -turret.angle;

            var turretForce = turretDeltaAngle * turretSpringForce;
            turretForce -= turret.velocity * turretSpringDrag;
            turret.velocity += turretForce * Time.fixedDeltaTime;

            if (float.IsNaN(turret.velocity))
                turret.velocity = 0;
            else if (Mathf.Abs(turret.velocity) > turretMaxVelocity)
                turret.velocity = Mathf.Sign(turret.velocity) * turretMaxVelocity;

            turret.angle += turret.velocity * Time.fixedDeltaTime;

            foreach (var barrel in turret.barrels) {

                if (!barrel.rotate)
                    continue;

                var barrelPosition = body.position + body.rotation * (turret.Position + turretRotation * barrel.Position);
                var barrelPlane = new Plane(body.rotation * turretRotation * Vector3.right, barrelPosition);
                var targetOnBarrelPlane = barrelPlane.ClosestPointOnPlane(target);
                //Draw.ingame.Line(debugTarget.position, targetOnBarrelPlane);

                var barrelTo = targetOnBarrelPlane - barrelPosition;
                var barrelRotation = Quaternion.Euler(barrel.angle, 0, 0);
                var barrelFrom = body.rotation * turretRotation * barrelRotation * Vector3.forward;
                var barrelDeltaAngle = !barrel.ignoreTarget ? Vector3.SignedAngle(barrelFrom, barrelTo, barrelPlane.normal) : -barrel.angle;

                var barrelForce = barrelDeltaAngle * barrelSpringForce;
                barrelForce -= barrel.velocity * barrelSpringDrag;
                barrel.velocity += barrelForce * Time.fixedDeltaTime;

                if (float.IsNaN(barrel.velocity))
                    barrel.velocity = 0;
                else if (Mathf.Abs(barrel.velocity) > barrelMaxVelocity)
                    barrel.velocity = Mathf.Sign(barrel.velocity) * barrelMaxVelocity;

                barrel.angle += barrel.velocity * Time.fixedDeltaTime;
                barrel.angle = Mathf.Clamp(barrel.angle, barrel.clamp[0], barrel.clamp[1]);
            }
        }
    }

    private void Update() {

        foreach (var wheel in wheels) {

            UpdateWheel(wheel);

            if (wheel.transform)
                wheel.transform.SetPositionAndRotation(wheel.position, wheel.rotation);
            else {
                var matrix = Matrix4x4.TRS(wheel.position, wheel.rotation, Vector3.one);
                using (Draw.ingame.WithMatrix(matrix)) {
                    Draw.ingame.SolidCircle(Vector3.zero, Vector3.right, wheel.radius);
                    Draw.ingame.Line(Vector3.zero, Vector3.forward * wheel.radius, Color.black);
                }
            }
        }

        RecordPosition();
        if (Application.isPlaying)
            UpdateSprings(true);
        else
            ResetSprings();
        UpdateBodyRotation();
        UpdateBodyPosition();

        UpdateHpTextPosition();

        foreach (var turret in turrets) {

            var turretPosition = body.position + body.rotation * turret.Position;
            var turretRotation = Quaternion.Euler(0, turret.angle, 0);
            if (turret.transform)
                turret.transform.localRotation = turretRotation;
            else {
                Draw.ingame.SolidCircle(turretPosition, body.up, .2f);
                Draw.ingame.Line(turretPosition, turretPosition + (body.rotation * turretRotation * Vector3.forward) * .2f, Color.black);
                if (drawTurretNames)
                    Draw.ingame.Label2D(turretPosition, turret.name, 14, LabelAlignment.Center);
            }

            foreach (var barrel in turret.barrels) {

                var barrelPosition = body.position + body.rotation * (turret.Position + turretRotation * barrel.Position);
                var barrelRotation = Quaternion.Euler(barrel.angle, 0, 0);

                if (barrel.transform)
                    barrel.transform.localRotation = barrelRotation;
                else {
                    //Draw.ingame.SphereOutline(barrelPosition, .1f);
                    var barrelEnd = barrelPosition + (body.rotation * turretRotation * barrelRotation * Vector3.forward) * .5f;
                    Draw.ingame.Line(barrelPosition, barrelEnd);
                    if (drawBarrelNames)
                        Draw.ingame.Label2D(Vector3.Lerp(barrelPosition, barrelEnd, .5f), barrel.name, 14, LabelAlignment.Center);
                }
            }
        }
    }

    [Command]
    public void Hit(float force) {
        if (hitPoints.Count > 0) {
            var hitPoint = hitPoints.Random();
            ApplyInstantaneousWorldForce(hitPoint.position, -hitPoint.forward * force);
            using (Draw.ingame.WithDuration(2))
                Draw.ingame.Ray(hitPoint.position, hitPoint.forward);
        }
    }
}