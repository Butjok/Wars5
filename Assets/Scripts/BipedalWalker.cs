using System;
using System.Collections;
using System.Data;
using Drawing;
using UnityEngine;
using UnityEngine.Serialization;

public class BipedalWalker : MonoBehaviour {

    public float location;
    public Vector2 footLocationRange = new(-1, 1);
    public Vector3? oldPosition;

    public const int left = 0, right = 1;
    public float[] feetLocations = new float[2];
    public IEnumerator[] feetMovementCoroutines = new IEnumerator[2];

    public float duration = .1f;
    public Easing.Name easing = Easing.Name.Linear;

    public Vector3[] feet = new Vector3[2];
    [FormerlySerializedAs("hips")] public Transform[] hipJoints = new Transform[2];
    public RaycastHit[] footRaycastHits = new RaycastHit[2];

    public float legLength = .5f;

    public Transform[] hipBones = new Transform[2];
    public Transform[] kneeBones = new Transform[2];
    public Transform[] feetBones = new Transform[2];

    public bool drawDebug = false;
    public float footRotationIntensity = .1f;

    public float stepSize = 1;
    public float resetLocation = 7;

    public float floatingFootDistanceThreshold = .1f;

    public void Start() {
        location = resetLocation;
        feetLocations[left] = feetLocations[right] = -10000;
    }

    public void Update() {

        if (oldPosition is { } actualOldPosition) {
            var deltaPosition = transform.position - actualOldPosition;
            location += Vector3.Dot(transform.forward, deltaPosition);
        }

        if (Input.GetKey(KeyCode.R)) {
            location = resetLocation;
            feetLocations[left] = feetLocations[right] = -10000;
        }

        for (var side = left; side <= right; side++) {
            var targetLocation = GetFootTargetLocation(side);
            if ((Math.Abs(feetLocations[side] - targetLocation) > Mathf.Epsilon || Vector3.Distance(feet[side], feetBones[side].position) > floatingFootDistanceThreshold)
                && feetMovementCoroutines[side] == null 
                && TryRaycastFootTargetPosition(side, targetLocation, out var hit)) {
                footRaycastHits[side] = hit;

                //Draw.ingame.Cross(footRaycastHits[side].point, .1f, Color.yellow);

                feetLocations[side] = targetLocation;
                var coroutine = feetMovementCoroutines[side] = MoveFoot(side);
                StartCoroutine(coroutine);
            }

            var hipJoint = hipJoints[side];
            // 2d space starting at the hip joint going right along X axis
            var hipCircle2d = (center: Vector2.zero, radius: legLength / 2);
            var footCircle2d = (center: new Vector2(Vector3.Distance(hipJoint.position, feet[side]), 0), radius: legLength / 2);

            var directionAlongLeg = (feet[side] - hipJoint.position).normalized;
            var bendForwardDirection = Vector3.Cross(directionAlongLeg, transform.right);

            Vector3 footPosition, kneePosition;

            var isTouchingGround = false;
            // bent leg
            if (Maths.IntersectCircles(hipCircle2d.center, hipCircle2d.radius, footCircle2d.center, footCircle2d.radius, out var intersectionPoint2d, out _)) {
                var hip2d = intersectionPoint2d - hipCircle2d.center;
                var projectedHip2d = Vector2.Dot(hip2d, Vector2.right) * Vector2.right;
                var bend2d = intersectionPoint2d - projectedHip2d;
                var bendAmount = bend2d.magnitude;
                footPosition = feet[side];
                kneePosition = Vector3.Lerp(hipJoint.position, feet[side], .5f) + bendAmount * bendForwardDirection;
                isTouchingGround = true;
            }
            // straight leg, cannot reach up to foot target
            else {
                footPosition = hipJoint.position + directionAlongLeg * legLength;
                kneePosition = Vector3.Lerp(hipJoint.position, footPosition, .5f);
            }

            if (drawDebug)
                using (Draw.ingame.WithLineWidth(2)) {
                    Draw.ingame.Line(hipJoint.position, kneePosition);
                    Draw.ingame.Line(kneePosition, footPosition);
                }

            var hipBone = hipBones[side];
            if (hipBone) {
                hipBone.position = hipJoints[side].position;
                var directionTowardsKnee = kneePosition - hipBone.position;
                var rotatedDirection = Vector3.Cross(directionTowardsKnee, transform.right);
                hipBone.rotation = Quaternion.LookRotation(rotatedDirection, directionTowardsKnee);
            }
            var kneeBone = kneeBones[side];
            if (kneeBone) {
                kneeBone.position = kneePosition;
                var directionTowardsFoot = footPosition - kneeBone.position;
                var rotatedDirection = Vector3.Cross(directionTowardsFoot, transform.right);
                kneeBone.rotation = Quaternion.LookRotation(rotatedDirection, directionTowardsFoot);
            }
            var footBone = feetBones[side];
            if (footBone) {
                footBone.position = footPosition;
                var targetRotation = Quaternion.LookRotation(isTouchingGround ? footRaycastHits[side].normal : kneePosition - footPosition, transform.forward);
                footBone.rotation = Quaternion.Slerp(footBone.rotation, targetRotation, Time.deltaTime * footRotationIntensity);
            }
        }

        if (drawDebug) {
            void DrawFoot(int side) {
                Draw.ingame.SolidCircleXZ(feet[side], .1f);
            }
            using (Draw.ingame.WithColor(Color.red))
                DrawFoot(left);
            using (Draw.ingame.WithColor(Color.green))
                DrawFoot(right);
        }

        var feetAverage = (feet[left] + feet[right]) / 2;
        var targetBodyPosition = transform.position.ToVector2().ToVector3() + Vector3.up * (feetAverage.y + height);
        body.position = Vector3.Lerp(body.position, targetBodyPosition, Time.deltaTime * bodyYPositionChangeIntensity);

        oldPosition = transform.position;
    }

    public float height = .5f;
    public Transform body;

    public bool TryRaycastFootTargetPosition(int side, float footLocation, out RaycastHit hit) {
        var origin = transform.position + transform.forward * (footLocation - location + stepForwardOffset) + transform.right * (side == left ? -1 : 1) * stepWidth;
        return Physics.Raycast(origin + Vector3.up * 100, Vector3.down, out hit, float.MaxValue, LayerMasks.Terrain | LayerMasks.Roads);
    }
    public float GetFootTargetLocation(int side) {
        return side == left ? (Mathf.Floor(location * stepSize) + .5f) / stepSize : Mathf.Floor(location * stepSize + .5f) / stepSize;
    }

    public float stepWidth = .5f;
    public float stepForwardOffset = 0;
    [FormerlySerializedAs("bodyHeightChangeIntensity")] public float bodyYPositionChangeIntensity = 10;

    public float stepDurationFactor = .5f;

    public IEnumerator MoveFoot(int side) {
        var startPosition = feet[side];
        var manualControl = GetComponent<ManualControl>();
        if (manualControl) {
            var speed = Mathf.Abs(manualControl.speed);
            if (speed > .1f) {
                var startTime = Time.time;
                var duration = stepDurationFactor / speed;
                //Debug.Log(1);
                while (Time.time < startTime + duration) {
                    var t = (Time.time - startTime) / duration;
                    t = stepCurve.Evaluate(t);
                    //t = Easing.Dynamic(easing, t);
                    feet[side] = Vector3.LerpUnclamped(startPosition, footRaycastHits[side].point, t);
                    yield return null;
                }
            }
        }
        feet[side] = footRaycastHits[side].point;
        feetMovementCoroutines[side] = null;
    }

    public AnimationCurve stepCurve = new();
}