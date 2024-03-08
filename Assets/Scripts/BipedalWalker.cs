using System;
using System.Collections;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

public class BipedalWalker : MonoBehaviour {

    public Vector3? oldPosition;

    public const int left = 0, right = 1;

    public RaycastHit[] footTargets = new RaycastHit[2];

    public float legLength = .5f;

    public Transform[] hipBones = new Transform[2];
    public Transform[] kneeBones = new Transform[2];
    public Transform[] feetBones = new Transform[2];
    public bool drawDebug = false;
    public float footRotationIntensity = .1f;
    
    public float stepLength = .5f;
    public float stepTriggerLength = .5f;

    public Color[] colors = { Color.red, Color.green, };
    public float maxDuration = .5f;

    public bool TryRaycast(int side, float direction, out RaycastHit hit) {
        var origin = (direction + stepForwardOffset) * transform.forward * stepLength + hipBones[side].position;
        return Physics.Raycast(origin + Vector3.up * 100, Vector3.down, out hit, float.MaxValue, LayerMasks.Terrain | LayerMasks.Roads);
    }

    public IEnumerator FootMovement(int side, RaycastHit target, float? maxDuration = null, Func<float,float> easing = null) {
        /*var startTime = Time.time;
        var oldTarget = footTargets[side];
        var manualControl = GetComponent<ManualControl>();
        var distance = Vector2.Distance(oldTarget.point.ToVector2(), target.point.ToVector2());
        var duration = Mathf.Min(maxDuration ?? this.maxDuration, distance / Mathf.Abs(manualControl.speed) * footSpeedFactor);
        if (Mathf.Abs(manualControl.speed) < .001f)
            duration = this.maxDuration;*/
        //Debug.Log(duration);
        /*while (Time.time - startTime < duration) {
            var t = (Time.time - startTime) / duration;
            // t = stepCurve.Evaluate(t);
            //t = useCurve ? stepCurve.Evaluate(t) : Easing.Dynamic(easing, t);
            t = (easing ?? stepCurve.Evaluate)(t);
            footTargets[side].point = Vector3.LerpUnclamped(oldTarget.point, target.point, t);
            footTargets[side].normal = Vector3.LerpUnclamped(oldTarget.normal, target.normal, t);
            yield return null;
        }*/
        footTargets[side] = target;
        yield break;
    }
    public IEnumerator FootMovement(int side, float direction, float? maxDuration = null, Func<float,float> easing = null) {
        return TryRaycast(side, direction, out var hit) ? FootMovement(side, hit, maxDuration, easing) : null;
    }
    public IEnumerator FeetResetMovement(float left, float right) {
        yield return FootMovement(0, left, feetResetDuration, Easing.InOutQuad);
        yield return FootMovement(1, right, feetResetDuration, Easing.InOutQuad);
    }

    public void ResetFeet() {
        coroutineStack.Clear();
        coroutineStack.Push(FeetResetMovement(.5f, -.5f));
    }

    public void Start() {
        ResetFeet();
    }

    public float footSpeedFactor = 1;

    public Stack<IEnumerator> coroutineStack = new();
    public float walkingAnimationSpeedFactor = 1;

    public float walkingLayerWeight = 0;
    public float walkingLayerChangeSpeed = 1;

    public void LateUpdate() {

        var leftFootOffset = transform.InverseTransformPoint(feetBones[left].position).z / stepLength;
        

        var manualControl = GetComponent<ManualControl>();
        var animator = GetComponent<Animator>();
        if (animator) {
            var walkingCycleAnimation = animator.runtimeAnimatorController.animationClips[0];
            var length = walkingCycleAnimation.length;
            //animator.SetFloat("Time", animator.GetFloat("Time") + Time.deltaTime);
            var targetWalkingLayerWeight = MathUtils.SmoothStep(.5f, 1, Mathf.Abs(manualControl.speed));
            var difference = Mathf.Abs(targetWalkingLayerWeight - walkingLayerWeight);
            var maxChangeThisFrame = Time.deltaTime * walkingLayerChangeSpeed;
            walkingLayerWeight = maxChangeThisFrame > difference ? targetWalkingLayerWeight : Mathf.Lerp(walkingLayerWeight, targetWalkingLayerWeight, maxChangeThisFrame / difference);
            animator.SetLayerWeight(animator.GetLayerIndex("WalkingLayer"), walkingLayerWeight);
            animator.SetFloat("Time", animator.GetFloat("Time") + manualControl.speed * walkingAnimationSpeedFactor * Time.deltaTime);
        }
        
        //Debug.Log(leftFootOffset + .5f);

        while (coroutineStack.Count > 0) {
            var top = coroutineStack.Peek();
            if (top != null && top.MoveNext()) {
                var value = top.Current;
                if (value != null && value is IEnumerator subCoroutine)
                    coroutineStack.Push(subCoroutine);
                break;
            }
            coroutineStack.Pop();
        }

        if (drawDebug)
            for (var side = left; side <= right; side++) {
                var hipWorldPosition = (hipBones[side].position + transform.forward * stepForwardOffset).ToVector2();
                Draw.ingame.SolidCircleXZ(footTargets[side].point, .1f, colors[side]);
                Draw.ingame.CircleXZ(hipWorldPosition.ToVector3(), stepTriggerLength, Color.white);
            }

        for (var side = left; side <= right; side++)
            if (coroutineStack.Count == 0 && Vector2.Distance(feetBones[side].position.ToVector2(), (hipBones[side].position + transform.forward * stepForwardOffset).ToVector2()) > stepLength) {
                if (side == left)
                    coroutineStack.Push(FootMovement(left, 1));
                else
                    coroutineStack.Push(FootMovement(right, leftFootOffset + 1));
            }

        /*if (transform.position == oldPosition && coroutineStack.Count == 0)
            coroutineStack.Push(FeetResetMovement(.5f, -.5f));*/

        for (var side = left; side <= right; side++) {

            var hipJoint = hipBones[side];
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
                hipBone.position = hipBones[side].position;
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
                //footBone.rotation = targetRotation;
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


    public float stepForwardOffset = 0;
    public float bodyYPositionChangeIntensity = 10;

    public float feetResetDuration = .25f;

    public AnimationCurve stepCurve = new();
}