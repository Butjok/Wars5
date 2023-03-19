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
using static UnitView.Wheel.Axis.Constants;

[ExecuteInEditMode]
public class UnitView : MonoBehaviour {

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

        public enum SteeringGroup {
            None, A, B, C, D
        }

        public const float rayOriginHeight = 1000;
        public bool IsSteeringWheel => steeringGroup != SteeringGroup.None;
        public int Side => raycastOrigin.x < 0 ? left : right;
        public float SpringTargetLength => -yOffset;

        public float radius = .1f;
        public Transform transform;
        public Vector2 raycastOrigin;
        public SteeringGroup steeringGroup = SteeringGroup.None;
        public bool isFixed = false;
        [FormerlySerializedAs("fixedHeight")] public float yOffset;

        [NonSerialized] public float spinAngle;
        [NonSerialized] public Vector3? previousPosition;
        [NonSerialized] public Vector3 position;
        [NonSerialized] public Quaternion rotation;
        [NonSerialized] public Vector3 springWeightPosition;
        [NonSerialized] public float springVelocity;
        [NonSerialized] public float steeringAngle;
        [NonSerialized] public Vector3 previousOriginPosition;
    }

    [Serializable]
    public class Turret {

        public string name = "Turret";
        public Transform transform;
        public Vector3 position;
        public WorkMode workMode = WorkMode.RotateToTarget;
        [NonSerialized] public float angle;
        public List<Barrel> barrels = new();
        [NonSerialized] public float velocity;

        [Serializable]
        public class Barrel {

            public string name = "MainGun";
            public Transform transform;
            public Vector3 position;
            [FormerlySerializedAs("clamp")] public Vector2 angleLimits = new(-5, 45);
            public WorkMode workMode = WorkMode.RotateToTarget;
            public ParticleSystem shotParticleSystem;
            public AudioSource audioSource;
            [NonSerialized] public float angle;
            [NonSerialized] public float velocity;
            public float recoil = 10;
        }
    }

    [Serializable]
    public struct Record {
        public string name;
        [TextArea(5, 10)] public string input;
    }

    public static UnitView DefaultPrefab => "WbLightTankRigged".LoadAs<UnitView>();

    [Command] public static bool drawTurretNames = false;
    [Command] public static bool drawBarrelNames = false;
    [Command] public static Vector2 wheelSteeringRange = new(-45, 45);
    [Command] public static float wheelSteeringDuration90 = .5f;
    [Command] public static float wheelFriction = 1;
    [Command] public static Vector2 terrainBumpRange = new(0, .02f);
    [Command] public static float terrainBumpTiling = 5f;
    [Command] public static float springForce = 250;
    [Command] public static float springDrag = 6;
    [Command] public static float maxSpringVelocity = 10;
    [Command] public static float accelerationCalculationTimeRange = .25f;
    [Command] public static float graphHeight = 300;
    [Command] public static float graphYHalfRange = 6;
    [Command] public static float turretSpringForce = 1000;
    [Command] public static float turretSpringDrag = 10;
    [Command] public static float turretMaxVelocity = 180;
    [Command] public static float barrelSpringForce = 1000;
    [Command] public static float barrelSpringDrag = 20;
    [Command] public static float barrelMaxVelocity = 90;
    [Command] public static float guiClippingDistance = .5f;
    [Command] public static string playerColorUniformName = "_PlayerColor";
    [Command] public static string attackHighlightFactorUniformName = "_AttackHighlightFactor";
    [Command] public static string attackHighlightStartTimeUniformName = "_AttackHighlightStartTime";
    [Command] public static string movedUniformName = "_Moved";
    [Command] public static bool drawAccelerationGraph;
    [Command] public static bool drawWheelCircleAnyway = false;
    [Command] public static float maxTorque = 25;
    [Command] public static float turnTorqueMultiplier = 0.125f;

    public static LayerMask TerrainLayerMask => LayerMasks.Terrain;

    public UnitView prefab;

    public Transform debugTarget;
    public Vector3 target;

    [Space]
    public Transform body;
    [FormerlySerializedAs("playerColorRenderers")]
    public List<Renderer> playerMaterialRenderers = new();
    public List<Wheel> wheels = new();
    public List<Turret> turrets = new();
    public float barrelRestAngle = -15;

    [Space]
    public float springLengthDelta = .05f;

    [Space]
    public Vector3 bodyCenterOfMass;
    public float accelerationTorqueMultiplier = 7.5f;

    [Space]
    public int maxHp = 10;
    public TMP_Text hpText;
    public Vector3 uiBoxCenter;
    public Vector3 uiBoxHalfSize = new(.5f, .5f, .5f);
    public bool drawUiBounds = false;
    public TMP_Text lowAmmoText;
    public TMP_Text lowFuelText;
    public List<TMP_Text> fadedTexts = new();

    [Space]
    public List<Record> subroutines = new();
    public WeaponNameBattleAnimationInputsDictionary inputs = new();
    [NonSerialized] public float speed;
    [NonSerialized] public float acceleration;
    [NonSerialized] public bool survives;
    public List<UnitView> targets = new();
    public List<Transform> hitPoints = new();
    [NonSerialized] public int incomingProjectilesLeft = 0;
    [NonSerialized] public int spawnPointIndex;
    [NonSerialized] public int shuffledIndex;

    private void Reset() {
        playerMaterialRenderers = GetComponentsInChildren<Renderer>().ToList();
        prefab = this;
    }

    [ContextMenu(nameof(FindHitPoints))]
    private void FindHitPoints() {
        hitPoints.Clear();
        hitPoints.AddRange(GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("HitPoint")));
    }

    private MaterialPropertyBlock materialPropertyBlock;
    private MaterialPropertyBlock MaterialPropertyBlock => materialPropertyBlock ??= new MaterialPropertyBlock();
    private void ApplyMaterialPropertyBlock() {
        foreach (var renderer in playerMaterialRenderers)
            renderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    [Command]
    public Color PlayerColor {
        get => MaterialPropertyBlock.GetColor(playerColorUniformName);
        set {
            MaterialPropertyBlock.SetColor(playerColorUniformName, value);
            ApplyMaterialPropertyBlock();
        }
    }

    public bool Selected { set { } }

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
        UpdateBodyPosition();
        UpdateBodyRotation();
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
    public void Die() {
        Visible = false;
    }

    [Command]
    public int Hp {
        get => hpText ? int.Parse(hpText.text) : 0;
        set {
            if (hpText) {
                hpText.text = value.ToString();
                hpText.enabled = value != maxHp;
            }
        }
    }
    [Command]
    public void AnimateHp(int value) {
        StartCoroutine(HpAnimation(value));
    }
    private IEnumerator HpAnimation(int to) {
        var from = Hp;
        if (from == to)
            yield break;
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

    public bool HasCargo {
        set { }
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

    [Command]
    public bool LowAmmo {
        get => lowAmmoText.enabled;
        set => lowAmmoText.enabled = value;
    }
    [Command]
    public bool LowFuel {
        get => lowFuelText.enabled;
        set => lowFuelText.enabled = value;
    }

    private void UpdateWheel(Wheel wheel) {

        var spinRotation = Quaternion.Euler(wheel.spinAngle, 0, 0);
        var steeringRotation = Quaternion.Euler(0, wheel.steeringAngle, 0);
        wheel.rotation = body.rotation * steeringRotation * spinRotation;

        var projectedOriginLocalPosition = new Vector3(wheel.raycastOrigin.x, 0, wheel.raycastOrigin.y);
        var projectedOriginWorldPosition = transform.TransformPoint(projectedOriginLocalPosition);
        var originWorldPosition = projectedOriginWorldPosition + Wheel.rayOriginHeight * Vector3.up;
        var ray = new Ray(originWorldPosition, Vector3.down);

        if (wheel.isFixed)
            wheel.position = body.position + body.rotation * (projectedOriginLocalPosition + Vector3.up * wheel.yOffset);

        else {
            var hasHit = Physics.SphereCast(ray, wheel.radius, out var hit, float.MaxValue, TerrainLayerMask);
            if (hasHit) {
                wheel.position = ray.GetPoint(hit.distance);
            }
            else {
                wheel.position.x = projectedOriginWorldPosition.x;
                wheel.position.z = projectedOriginWorldPosition.z;
            }

            var noise = Mathf.PerlinNoise(wheel.position.x * terrainBumpTiling, wheel.position.z * terrainBumpTiling);
            var height = Mathf.Lerp(terrainBumpRange[0], terrainBumpRange[1], noise);
            wheel.position += height * Vector3.up;
        }

        if (wheel.previousPosition is { } actualPreviousPosition) {
            // spin then wheel
            var wheelForward = body.rotation * steeringRotation * Vector3.forward;
            var deltaPosition = wheel.position - actualPreviousPosition;
            var distance = Vector3.Dot(deltaPosition, wheelForward);
            const float ratio = 180 / Mathf.PI;
            var deltaAngle = distance / wheel.radius * ratio;
            wheel.spinAngle += deltaAngle;
        }
        wheel.previousPosition = wheel.position;

        if (wheel.IsSteeringWheel && wheel.previousOriginPosition is { } actualPreviousOriginPosition) {
            var from = Quaternion.Euler(0, wheel.steeringAngle, 0);
            var delta = projectedOriginWorldPosition - actualPreviousOriginPosition;
            if (delta != Vector3.zero) {
                var to = (Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(delta, Vector3.up)).normalized;
                var sectorAngle = Quaternion.Angle(from, to);
                var sectorDuration = wheelSteeringDuration90 * sectorAngle / 90;
                var rotation = Quaternion.Slerp(from, to, wheelFriction * (1 / sectorDuration) * (delta.magnitude));
                wheel.steeringAngle = rotation.eulerAngles.y;
                while (wheel.steeringAngle > 180)
                    wheel.steeringAngle -= 360;
                while (wheel.steeringAngle < -180)
                    wheel.steeringAngle += 360;
                wheel.steeringAngle = Mathf.Clamp(wheel.steeringAngle, wheelSteeringRange[0], wheelSteeringRange[1]);
            }
        }
        wheel.previousOriginPosition = projectedOriginWorldPosition;
    }

    private Dictionary<int, Wheel.Axis> axes = new();
    private List<Wheel.Axis> pitchAxes = new();

    [ContextMenu(nameof(UpdateAxes))]
    private void UpdateAxes() {

        axes.Clear();
        foreach (var wheel in wheels) {
            var y = Mathf.RoundToInt(wheel.raycastOrigin.y * 100);
            if (!axes.TryGetValue(y, out var axis)) {
                axis = axes[y] = new Wheel.Axis();
            }
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

        if (axes.Count > 0 && pitchAxes.Count > 0) {

            var rightSum = Vector3.zero;
            foreach (var axis in axes.Values.Where(axis => !axis[left].isFixed && !axis[right].isFixed))
                rightSum += axis[right].springWeightPosition - axis[left].springWeightPosition;
            rightSum /= axes.Count;

            var forwardSum = Vector3.zero;
            foreach (var axis in pitchAxes.Where(axis => !axis[left].isFixed && !axis[right].isFixed))
                forwardSum += axis[front].springWeightPosition - axis[back].springWeightPosition;
            forwardSum /= pitchAxes.Count;

            var up = Vector3.Cross(rightSum, forwardSum);
            if (Vector3.Dot(Vector3.up, up) < 0)
                up = -up;

            body.rotation = Quaternion.LookRotation(forwardSum, up);
        }
    }

    private Queue<Vector3> instantaneousTorques = new();
    private void UpdateSpring(Wheel wheel, Vector3 accelerationTorque, Vector3 turnTorque, float deltaTime) {

        if (wheel.isFixed)
            return;

        var springLength = Vector3.Dot(body.up, wheel.springWeightPosition - wheel.position);
        var springForce = (wheel.SpringTargetLength - springLength) * UnitView.springForce;
        springForce -= wheel.springVelocity * springDrag;

        var centerOfMass = body.position + body.rotation * bodyCenterOfMass;

        void AddTorque(Vector3 torque) {
            // Draw.ingame.Arrow(centerOfMass, centerOfMass + torque);
            var force = Vector3.Dot(Vector3.Cross(torque, wheel.position - centerOfMass), body.up);
            springForce += force;
            // Draw.ingame.Arrow(wheel.position, wheel.position + body.up * force);
        }

        AddTorque(accelerationTorque);
        AddTorque(turnTorque);

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
        var a = wheel.SpringTargetLength - springLengthDelta;
        var b = wheel.SpringTargetLength + springLengthDelta;
        springLength = Mathf.Clamp(springLength, Mathf.Min(a, b), Mathf.Max(a, b));

        wheel.springWeightPosition = wheel.position + body.up * springLength;
    }

    private void UpdateBodyPosition() {
        if (wheels.Count <= 0)
            return;

        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var wheel in wheels.Where(wheel => !wheel.isFixed)) {
            min = Vector3.Min(min, wheel.springWeightPosition);
            max = Vector3.Max(max, wheel.springWeightPosition);
        }
        body.position = Vector3.Lerp(min, max, .5f);
    }

    private float? lastSpringUpdateTime;
    private void UpdateSprings(bool isCalledFromUpdate) {
        if (lastSpringUpdateTime is { } actualLastTime) {

            Vector3 accelerationTorque, turnTorque;
            if (TryCalculateAcceleration(isCalledFromUpdate && drawAccelerationGraph, out var linearSpeed, out var linearAcceleration, out var angularSpeed)) {
                accelerationTorque = -body.right * Mathf.Clamp(linearAcceleration * accelerationTorqueMultiplier, -maxTorque, maxTorque);
                turnTorque = body.forward * Mathf.Clamp(angularSpeed * turnTorqueMultiplier * linearSpeed, -maxTorque, maxTorque);
            }
            else {
                accelerationTorque = Vector3.zero;
                turnTorque = Vector3.zero;
            }


            foreach (var wheel in wheels)
                UpdateSpring(wheel, accelerationTorque, turnTorque, Time.time - actualLastTime);
            instantaneousTorques.Clear();
        }
        lastSpringUpdateTime = Time.time;
    }

    private List<(float time, float position)> accelerationPoints = new();
    private bool TryCalculateAcceleration(bool canDrawGraph, out float linearSpeed, out float linearAcceleration, out float angularSpeed) {

        float GetXInGraphSpace(float time) {
            var deltaTime = Time.time - time;
            return 1 - deltaTime / accelerationCalculationTimeRange;
        }

        float GetYInGraphSpace(float position) {
            return position / graphYHalfRange;
        }

        float GetXInScreenSpace(float xInGraphSpace) {
            return Screen.width * xInGraphSpace;
        }

        float GetYInScreenSpace(float yInGraphSpace) {
            return graphHeight / 2 + (yInGraphSpace / graphYHalfRange) * Screen.height;
        }

        Vector3 GetScreenPosition((float time, float position) point) {
            return new Vector3(
                GetXInScreenSpace(GetXInGraphSpace(point.time)),
                GetYInScreenSpace(GetYInGraphSpace(point.position)));
        }

        using (canDrawGraph ? (IDisposable)Draw.ingame.InScreenSpace(Camera.main) : new CommandBuilder.ScopeEmpty())
        using (canDrawGraph ? (IDisposable)Draw.ingame.WithLineWidth(1.5f) : new CommandBuilder.ScopeEmpty()) {

            linearSpeed = angularSpeed = linearAcceleration = 0;

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

            if (!Quadratic.TryApproximate(points, out var linearPositionApproximation))
                return false;

            var startTime = points[0].time;
            var endTime = points[^1].time;
            
            linearSpeed = linearPositionApproximation.FirstDerivative(endTime);
            linearAcceleration = linearPositionApproximation.SecondDerivative;

            if (canDrawGraph) {
                foreach (var point in points)
                    Draw.ingame.SolidCircleXY(GetScreenPosition(point), 5, Color.white);

                
                for (var time = startTime; time <= endTime; time += (endTime - startTime) / 10)
                    Draw.ingame.SolidCircleXY(GetScreenPosition((time, pos: linearPositionApproximation[time])), 1, Color.cyan);

                Draw.ingame.Label2D(new Vector3(50, 50), $"speed: {speed:0.##}\nacceleration: {linearAcceleration:0.##}");
                Draw.ingame.SolidCircleXY(GetScreenPosition((endTime - accelerationCalculationTimeRange / 2, linearAcceleration)), 10, Color.red);
            }

            yawDeltas.Clear();
            yawDeltas.Add((startTime, 0));
            var currentAngle = 0f;
            for (var i = 1; i < positions.Count; i++) {
                var delta = Vector3.SignedAngle(positions[i - 1].forward, positions[i].forward, Vector3.up);
                currentAngle += delta;
                yawDeltas.Add((positions[i].time, currentAngle));
            }

            if (Quadratic.TryApproximate(yawDeltas, out var yawApproximation))
                angularSpeed = yawApproximation[endTime];

            return true;
        }
    }

    private List<(float time, float value)> yawDeltas = new();
    private List<(float time, Vector3 position, Vector3 forward)> positions = new();

    private void RecordPosition() {
        positions.RemoveAll(item => item.time < Time.time - accelerationCalculationTimeRange);
        positions.Add((Time.time, transform.position, transform.forward));
    }

    public void ResetSprings() {
        foreach (var wheel in wheels) {
            wheel.springVelocity = 0;
            wheel.springWeightPosition = wheel.position + body.up * wheel.SpringTargetLength;
        }
        instantaneousTorques.Clear();
        positions.Clear();
    }

    public void ResetSteering() {
        foreach (var wheel in wheels)
            wheel.steeringAngle = 0;
    }

    public void ApplyInstantaneousTorque(Vector3 position, Vector3 force) {
        var centerOfMass = body.position + body.rotation * bodyCenterOfMass;
        var torque = Vector3.Cross(position - centerOfMass, force);
        instantaneousTorques.Enqueue(torque);
    }
    [Command]
    public void ApplyInstantaneousLocalForce(Vector3 localPosition, Vector3 localForce) {
        ApplyInstantaneousTorque(transform.TransformPoint(localPosition), transform.TransformPoint(localForce));
    }
    public void ApplyInstantaneousWorldForce(Vector3 position, Vector3 force) {
        ApplyInstantaneousTorque(position, force);
    }
    public void Shoot(Turret turret, Turret.Barrel barrel) {

        var turretRotation = Quaternion.Euler(0, turret.angle, 0);
        var barrelPosition = body.position + body.rotation * (turret.position + turretRotation * barrel.position);
        var barrelRotation = Quaternion.Euler(barrel.angle, 0, 0);

        var barrelForward = body.rotation * turretRotation * barrelRotation * Vector3.forward;
        //using (Draw.ingame.WithDuration(2))
        //	Draw.ingame.Arrow(barrelPosition, barrelPosition + barrelForward, Color.yellow);

        if (barrel.shotParticleSystem)
            barrel.shotParticleSystem.Play();
        if (barrel.audioSource)
            barrel.audioSource.Play();

        ApplyInstantaneousWorldForce(barrelPosition, -barrelForward * barrel.recoil);
    }
    [Command]
    public void Shoot(string turretName, string barrelName) {
        var turret = turrets.SingleOrDefault(t => t.name == turretName);
        var barrel = turret?.barrels.SingleOrDefault(b => b.name == barrelName);
        if (barrel != null)
            Shoot(turret, barrel);
    }

    private void OnEnable() {
        FindHitPoints();
        UpdateAxes();
        PlaceOnTerrain();
    }

    private void UpdateHpTextPosition() {
        if (!hpText)
            return;
        if (Hp != maxHp && hpText.rectTransform.TryEncapsulate(UiBoundPoints, out var distance)) {
            hpText.gameObject.SetActive(true);
            fadedTexts.FadeAlpha(distance);
        }
        else
            hpText.gameObject.SetActive(false);
    }

    private void FixedUpdate() {

        if (!body)
            return;

        UpdateSprings(false);

        if (debugTarget)
            target = debugTarget.position;

        foreach (var turret in turrets) {

            if (turret.workMode == WorkMode.Idle)
                continue;

            var turretPosition = body.position + body.rotation * turret.position;
            var turretPlane = new Plane(body.up, turretPosition);
            var targetOnTurretPlane = turretPlane.ClosestPointOnPlane(target);
            var turretTo = targetOnTurretPlane - turretPosition;
            var turretRotation = Quaternion.Euler(0, turret.angle, 0);
            var turretFrom = body.rotation * turretRotation * Vector3.forward;
            var turretDeltaAngle = turret.workMode == WorkMode.RotateToRest ? -turret.angle : Vector3.SignedAngle(turretFrom, turretTo, turretPlane.normal);

            var turretForce = turretDeltaAngle * turretSpringForce;
            turretForce -= turret.velocity * turretSpringDrag;
            turret.velocity += turretForce * Time.fixedDeltaTime;

            if (float.IsNaN(turret.velocity))
                turret.velocity = 0;
            else if (Mathf.Abs(turret.velocity) > turretMaxVelocity)
                turret.velocity = Mathf.Sign(turret.velocity) * turretMaxVelocity;

            turret.angle += turret.velocity * Time.fixedDeltaTime;

            foreach (var barrel in turret.barrels) {

                if (barrel.workMode == WorkMode.Idle)
                    continue;

                var barrelPosition = body.position + body.rotation * (turret.position + turretRotation * barrel.position);
                var barrelPlane = new Plane(body.rotation * turretRotation * Vector3.right, barrelPosition);
                var targetOnBarrelPlane = barrelPlane.ClosestPointOnPlane(target);
                //Draw.ingame.Line(debugTarget.position, targetOnBarrelPlane);

                var barrelTo = targetOnBarrelPlane - barrelPosition;
                var barrelRotation = Quaternion.Euler(barrel.angle, 0, 0);
                var barrelFrom = body.rotation * turretRotation * barrelRotation * Vector3.forward;
                var barrelDeltaAngle = barrel.workMode == WorkMode.RotateToRest ? barrelRestAngle - barrel.angle : Vector3.SignedAngle(barrelFrom, barrelTo, barrelPlane.normal);

                var barrelForce = barrelDeltaAngle * barrelSpringForce;
                barrelForce -= barrel.velocity * barrelSpringDrag;
                barrel.velocity += barrelForce * Time.fixedDeltaTime;

                if (float.IsNaN(barrel.velocity))
                    barrel.velocity = 0;
                else if (Mathf.Abs(barrel.velocity) > barrelMaxVelocity)
                    barrel.velocity = Mathf.Sign(barrel.velocity) * barrelMaxVelocity;

                barrel.angle += barrel.velocity * Time.fixedDeltaTime;
                barrel.angle = Mathf.Clamp(barrel.angle, -barrel.angleLimits[1], -barrel.angleLimits[0]);
            }
        }
    }

    private Dictionary<Wheel.SteeringGroup, (float angleAccumulator, int count)> steeringGroups = new();
    private void Update() {

        if (!body)
            return;

        if (!Application.isPlaying)
            UpdateAxes();

        speed += acceleration * Time.deltaTime;
        transform.position += transform.forward * speed * Time.deltaTime;

        steeringGroups.Clear();

        foreach (var wheel in wheels) {
            UpdateWheel(wheel);

            if (wheel.IsSteeringWheel) {
                if (!steeringGroups.TryGetValue(wheel.steeringGroup, out var group))
                    group = steeringGroups[wheel.steeringGroup] = (0, 0);
                group.count++;
                group.angleAccumulator += wheel.steeringAngle;
                steeringGroups[wheel.steeringGroup] = group;
            }
        }

        foreach (var wheel in wheels) {

            if (wheel.IsSteeringWheel && steeringGroups.TryGetValue(wheel.steeringGroup, out var group))
                wheel.steeringAngle = group.angleAccumulator / group.count;

            var matrix = Matrix4x4.TRS(wheel.position, wheel.rotation, Vector3.one);
            using (Draw.ingame.WithMatrix(matrix)) {
                if (wheel.transform) {
                    wheel.transform.SetPositionAndRotation(wheel.position, wheel.rotation);
                    if (!Application.isPlaying || drawWheelCircleAnyway)
                        Draw.ingame.Circle(Vector3.zero, Vector3.right, wheel.radius);
                }
                else {
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

            var turretPosition = body.position + body.rotation * turret.position;
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

                var barrelPosition = body.position + body.rotation * (turret.position + turretRotation * barrel.position);
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
    public void TakeHit(float force) {
        if (hitPoints.Count <= 0)
            return;
        TakeHit(hitPoints.Random(), force);
    }

    public void TakeHit(Transform hitPoint, float force) {
        ApplyInstantaneousWorldForce(hitPoint.position, -hitPoint.forward * force);
        using (Draw.ingame.WithDuration(2))
            Draw.ingame.Ray(hitPoint.position, hitPoint.forward);
    }

    [Command]
    public void SetWorkMode(WorkMode workMode) {
        foreach (var turret in turrets) {
            turret.workMode = workMode;
            foreach (var barrel in turret.barrels)
                barrel.workMode = workMode;
        }
    }

    [Command]
    public Func<bool> Play(string input) {
        var completed = false;
        StartCoroutine(PlayAnimation(input, () => completed = true));
        return () => completed;
    }

    public WeaponName GetFallbackWeaponName(WeaponName weaponName) {
        if (name.StartsWith("WbLightTankRigged"))
            if (weaponName == WeaponName.Rifle)
                weaponName = WeaponName.MachineGun;
            else if (weaponName == WeaponName.RocketLauncher)
                weaponName = WeaponName.Cannon;
        return weaponName;
    }

    public string GetDefaultMoveAttackInput(WeaponName weaponName) {
        weaponName = GetFallbackWeaponName(weaponName);
        return "reset-weapons 1.5 .5 2 move-in " + GetDefaultAttackInput(weaponName);
    }
    public int automaticWeaponShotsCount = 5;
    public string GetDefaultAttackInput(WeaponName weaponName) {
        weaponName = GetFallbackWeaponName(weaponName);
        var shotsCount = weaponName is WeaponName.Rifle or WeaponName.MachineGun ? automaticWeaponShotsCount : 1;
        var loop = Enumerable.Repeat($"Main {weaponName} shoot .1 wait", shotsCount);
        return $"reset-weapons .33 wait Main _ aim .33 wait Main {weaponName} aim .66 wait " + string.Join(" ", loop);
    }
    public string GetDefaultResponseInput(WeaponName weaponName) {
        weaponName = GetFallbackWeaponName(weaponName);
        return GetDefaultAttackInput(weaponName);
    }

    [Command]
    public Func<bool> MoveAttack(WeaponName weaponName) {
        return Play(inputs.TryGetValue(weaponName, out var record) ? record.moveAttack : GetDefaultMoveAttackInput(weaponName));
    }
    [Command]
    public Func<bool> Attack(WeaponName weaponName) {
        return Play(inputs.TryGetValue(weaponName, out var record) ? record.attack : GetDefaultAttackInput(weaponName));
    }
    [Command]
    public Func<bool> Respond(WeaponName weaponName) {
        return Play(inputs.TryGetValue(weaponName, out var record) ? record.respond : GetDefaultResponseInput(weaponName));
    }

    [Command]
    public void Translate(float value) {
        transform.position += transform.forward * value;
        PlaceOnTerrain();
    }

    public IEnumerator Break(float acceleration) {
        this.acceleration = -Mathf.Sign(speed) * acceleration;
        yield return new WaitForSeconds(BreakTime(speed, this.acceleration));
        this.acceleration = 0;
        speed = 0;
    }

    [Command]
    public static float BreakTime(float speed, float acceleration) {
        return Mathf.Abs(speed / acceleration);
    }

    public IEnumerator MoveIn(float speed, float time, float acceleration) {
        var breakTime = BreakTime(speed, acceleration);
        var distance = speed * time + speed * breakTime - acceleration * breakTime * breakTime / 2;
        Translate(-distance);
        this.speed = speed;
        yield return new WaitForSeconds(time);
        yield return Break(acceleration);
    }

    [Command]
    public void ResetWeapons() {
        foreach (var turret in turrets) {
            turret.angle = 0;
            turret.workMode = WorkMode.RotateToRest;
            foreach (var barrel in turret.barrels) {
                barrel.angle = barrelRestAngle;
                barrel.workMode = WorkMode.RotateToRest;
            }
        }
    }

    private IEnumerator PlayAnimation(string input, Action onComplete = null, int level = 0, WarsStack stack = null) {

        stack ??= new WarsStack();

        foreach (var token in Tokenizer.Tokenize(input))
            switch (token) {

                case "reset-weapons": {
                    ResetWeapons();
                    break;
                }

                case "rest":
                case "aim": {
                    var barrelName = stack.Pop<string>();
                    var turretName = stack.Pop<string>();

                    var turret = turrets.SingleOrDefault(t => t.name == turretName);
                    if (turret != null) {
                        var barrel = turret.barrels.SingleOrDefault(b => b.name == barrelName);
                        var workMode = token == "aim" ? WorkMode.RotateToTarget : WorkMode.RotateToRest;
                        if (barrel != null)
                            barrel.workMode = workMode;
                        else
                            turret.workMode = workMode;
                    }
                    break;
                }

                case "move-in": {
                    var acceleration = stack.Pop<dynamic>();
                    var time = stack.Pop<dynamic>();
                    var speed = stack.Pop<dynamic>();
                    yield return MoveIn(speed, time, acceleration);
                    break;
                }

                case "spawn-point-index": {
                    stack.Push(spawnPointIndex);
                    break;
                }

                case "shuffled-index": {
                    stack.Push(shuffledIndex);
                    break;
                }

                case "wait": {
                    yield return new WaitForSeconds(stack.Pop<dynamic>());
                    break;
                }

                case "shoot": {
                    var barrelName = stack.Pop<string>();
                    var turretName = stack.Pop<string>();
                    Shoot(turretName, barrelName);
                    break;
                }

                case "call": {
                    var name = stack.Pop<string>();
                    var record = subroutines.SingleOrDefault(r => r.name == name);
                    Assert.IsTrue(!string.IsNullOrEmpty(record.name), $"cannot find subroutine {name}");
                    yield return PlayAnimation(record.input, onComplete, level + 1, stack);
                    break;
                }

                default:
                    stack.ExecuteToken(token);
                    break;
            }

        if (level == 0)
            onComplete?.Invoke();
    }

    public IEnumerable<Vector3> UiBoundPoints {
        get {
            for (var x = -1; x <= 1; x += 2)
            for (var y = -1; y <= 1; y += 2)
            for (var z = -1; z <= 1; z += 2) {
                var localPosition = uiBoxHalfSize;
                localPosition.Scale(new Vector3(x, y, z));
                localPosition += uiBoxCenter;
                yield return body.position + body.rotation * localPosition;
            }
        }
    }
}

public enum WorkMode {
    RotateToRest, RotateToTarget, Idle
}