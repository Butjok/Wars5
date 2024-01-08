using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

    public RaycastHit[] footTargets = new RaycastHit[2];
    public Transform[] hipJoints = new Transform[2];

    public float legLength = .5f;

    public Transform[] hipBones = new Transform[2];
    public Transform[] kneeBones = new Transform[2];
    public Transform[] feetBones = new Transform[2];

    public bool drawDebug = false;
    public float footRotationIntensity = .1f;

    public float stepSize = 1;
    public float resetLocation = 7;

    public float floatingFootDistanceThreshold = .1f;

    public float stepLength = .5f;
    public float stepTriggerLength = .5f;
    public IEnumerator footMovementCoroutine = null;
    public float feetTargetMovementSpeed = 1;
    public int[] sign = new int[2];
    public int[] previousSign = new int[2];

    public float minStepLength = .25f;

    public Color[] colors = { Color.red, Color.green, };
    public float maxDuration = .5f;
    
    public Easing.Name easing = Easing.Name.InOutQuad;
    public bool useCurve = false;

    public bool TryRaycast(int side, float direction, out RaycastHit hit) {
        var origin = (direction + stepForwardOffset) * transform.forward * stepLength + hipJoints[side].position;
        return Physics.Raycast(origin + Vector3.up * 100, Vector3.down, out hit, float.MaxValue, LayerMasks.Terrain | LayerMasks.Roads);
    }

    public IEnumerator FootMovement(int side, RaycastHit target, bool clearOnFinish = true) {
        var startTime = Time.time;
        var oldTarget = footTargets[side];
        var manualControl = GetComponent<ManualControl>();
        var distance = Vector2.Distance(oldTarget.point.ToVector2(), target.point.ToVector2());
        var duration = Mathf.Min(maxDuration, distance / Mathf.Abs(manualControl.speed) * footSpeedFactor);
        while (Time.time - startTime < duration) {
            var t = (Time.time - startTime) / duration;
            // t = stepCurve.Evaluate(t);
            t = useCurve ? stepCurve.Evaluate(t) : Easing.Dynamic(easing, t);
            footTargets[side].point = Vector3.LerpUnclamped(oldTarget.point, target.point, t);
            footTargets[side].normal = Vector3.LerpUnclamped(oldTarget.normal, target.normal, t);
            yield return null;
        }
        footTargets[side] = target;
        if (clearOnFinish)
            footMovementCoroutine = null;
    }
    public IEnumerator FootMovement(int side, float direction, bool clearOnFinish = true) {
        return TryRaycast(side, direction, out var hit) ? FootMovement(side, hit, clearOnFinish) : null;
    }
    public IEnumerator FeetMovement(float left, float right) {
        yield return FootMovement(0, left, false);
        yield return FootMovement(1, right);
    }

    public void Start() {
        footMovementCoroutine = FeetMovement(.5f, -.5f);
        StartCoroutine(footMovementCoroutine);
    }

    public float footSpeedFactor = 1;
    public float minFootSpeed = 10;

    public void Update() {

        if (drawDebug)
            for (var side = left; side <= right; side++) {
                var hipWorldPosition = (hipJoints[side].position + transform.forward * stepForwardOffset).ToVector2();
                Draw.ingame.SolidCircleXZ(footTargets[side].point, .1f, colors[side]);
                Draw.ingame.CircleXZ(hipWorldPosition.ToVector3(), stepTriggerLength, Color.white);
            }

        if (footMovementCoroutine == null) {

            for (var side = left; side <= right; side++)
                if (footMovementCoroutine == null && Vector2.Distance(feetBones[side].position.ToVector2(), (hipJoints[side].position + transform.forward * stepForwardOffset).ToVector2()) > stepLength) {
                    if (side == left) {
                        footMovementCoroutine = FootMovement(side, 1);
                        StartCoroutine(footMovementCoroutine);
                    }
                    else {
                        var leftFootOffset = transform.InverseTransformPoint(feetBones[left].position).z / stepLength;
                        footMovementCoroutine = FootMovement(right, leftFootOffset + 1);
                        StartCoroutine(footMovementCoroutine);
                    }
                }

            if (transform.position == oldPosition && footMovementCoroutine == null) {
                footMovementCoroutine = FeetMovement(.5f, -.5f);
                StartCoroutine(footMovementCoroutine);
            }
        }

        for (var side = left; side <= right; side++) {

            var hipJoint = hipJoints[side];
            // 2d space starting at the hip joint going right along X axis
            var hipCircle2d = (center: Vector2.zero, radius: legLength / 2);
            var footCircle2d = (center: new Vector2(Vector3.Distance(hipJoint.position, footTargets[side].point), 0), radius: legLength / 2);

            var directionAlongLeg = (footTargets[side].point - hipJoint.position).normalized;
            var bendForwardDirection = Vector3.Cross(directionAlongLeg, transform.right);

            Vector3 footPosition, kneePosition;

            var isTouchingGround = false;
            // bent leg
            if (Maths.IntersectCircles(hipCircle2d.center, hipCircle2d.radius, footCircle2d.center, footCircle2d.radius, out var intersectionPoint2d, out _)) {
                var hip2d = intersectionPoint2d - hipCircle2d.center;
                var projectedHip2d = Vector2.Dot(hip2d, Vector2.right) * Vector2.right;
                var bend2d = intersectionPoint2d - projectedHip2d;
                var bendAmount = bend2d.magnitude;
                footPosition = footTargets[side].point;
                kneePosition = Vector3.Lerp(hipJoint.position, footTargets[side].point, .5f) + bendAmount * bendForwardDirection;
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
                var targetRotation = Quaternion.LookRotation(isTouchingGround ? footTargets[side].normal : kneePosition - footPosition, transform.forward);
                footBone.rotation = Quaternion.Slerp(footBone.rotation, targetRotation, Time.deltaTime * footRotationIntensity);
            }
        }

        if (drawDebug) {
            void DrawFoot(int side) {
                Draw.ingame.SolidCircleXZ(footTargets[side].point, .1f);
            }
            using (Draw.ingame.WithColor(Color.red))
                DrawFoot(left);
            using (Draw.ingame.WithColor(Color.green))
                DrawFoot(right);
        }

        var feetAverage = (footTargets[left].point + footTargets[right].point) / 2;
        var targetBodyPosition = transform.position.ToVector2().ToVector3() + Vector3.up * (feetAverage.y + height);
        body.position = Vector3.Lerp(body.position, targetBodyPosition, Time.deltaTime * bodyYPositionChangeIntensity);

        oldPosition = transform.position;
    }

    public float height = .5f;
    public Transform body;


    public float stepWidth = .5f;
    public float stepForwardOffset = 0;
    [FormerlySerializedAs("bodyHeightChangeIntensity")]
    public float bodyYPositionChangeIntensity = 10;

    public float stepDurationFactor = .5f;


    public AnimationCurve stepCurve = new();
}