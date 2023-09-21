using System;
using Butjok.CommandLine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public class MissileSiloView : BuildingView {

    public Transform target;
    public Vector3 targetPosition;
    public Transform barrel;
    public Vector2 speeds = new(180, 90);
    public Vector2 angleBounds = new(0, 80);
    public float minVelocity = 3;
    public float velocityStep = .1f;
    public float gravity = 5;
    public bool aim = true;
    public BallisticMotion projectilePrefab;
    public bool selectHighCurve = true;

    public Vector2Int lookDirection;

    public override Vector2Int LookDirection {
        get => base.LookDirection;
        set {
            base.LookDirection = value;
            lookDirection = value;
        }
    }

    [Command]
    public void ToggleAim() {
        aim = !aim;
    }

    private void Update() {

        if (target)
            targetPosition = target.position;

        if (TryCalculateHorizontalTargetRotation(out var horizontalTargetRotation))
            transform.rotation = transform.rotation.SlerpWithMaxSpeed(horizontalTargetRotation, speeds[0]);

        barrel.localRotation = barrel.localRotation.SlerpWithMaxSpeed(
            aim && TryCalculateBarrelTargetLocalRotation(out var barrelTargetLocalRotation)
                ? barrelTargetLocalRotation
                : Quaternion.identity, speeds[1]);
    }

    public bool TryCalculateHorizontalTargetRotation(out Quaternion targetRotation) {
        targetRotation = Quaternion.identity;

        var groundPlane = new Plane(transform.up, transform.position);
        var targetPositionOnGroundPlane = groundPlane.ClosestPointOnPlane(aim ? targetPosition : transform.position + lookDirection.ToVector3Int());
        var newForward = targetPositionOnGroundPlane - transform.position;
        if (newForward == Vector3.zero)
            return false;

        targetRotation = Quaternion.LookRotation(newForward, transform.up);
        return true;
    }

    public bool TryCalculateBarrelTargetLocalRotation(out Quaternion rotation) {

        rotation = Quaternion.identity;
        if (!TryCalculateCurve(out var curve))
            return false;

        //foreach (var (start, end) in curve.Segments())
          //  Draw.ingame.Line(start, end, Color.red);

        var aimingPosition = transform.position + curve.forward * Mathf.Cos(curve.theta) + curve.up * Mathf.Sin(curve.theta);

        var angle = Vector3.SignedAngle(transform.forward, aimingPosition - transform.position, transform.right);
        var clampedAngle = Mathf.Clamp(angle, angleBounds[0], angleBounds[1]);

        rotation = Quaternion.Euler(clampedAngle, 0, 0);
        return true;
    }

    public void SnapToTargetRotationInstantly() {

        if (TryCalculateHorizontalTargetRotation(out var horizontalTargetRotation))
            transform.rotation = horizontalTargetRotation;

        barrel.localRotation = aim && TryCalculateBarrelTargetLocalRotation(out var barrelTargetLocalRotation)
            ? barrelTargetLocalRotation
            : Quaternion.identity;
    }

    [Command]
    public BallisticMotion TryLaunchMissile() {

        if (!TryCalculateCurve(out var curve))
            return null;

        var projectile = Instantiate(projectilePrefab, transform.position, barrel.rotation);
        projectile.gameObject.SetActive(true);
        projectile.time = 0;
        projectile.curve = curve;
        return projectile;
    }

    public bool TryCalculateCurve(out BallisticCurve curve) {

        var stepsTaken = 0;
        for (var velocity = minVelocity;; velocity += velocityStep, stepsTaken++) {

            Assert.IsTrue(stepsTaken < 10000, "too many steps taken to calculate the minimum projectile velocity");

            if (!BallisticCurve.TryCalculate(transform.position, targetPosition, velocity, Vector3.down * gravity, out var curveLow, out var curveHigh))
                continue;

            curve = selectHighCurve ? curveHigh : curveLow;

            // hack to being able to fire straight up
            if (curve.totalTime is { } totalTime && Mathf.Approximately(0, totalTime) && Mathf.Approximately(curve.theta, Mathf.PI / 2))
                curve.totalTime = 2 * velocity / gravity;

            return true;
        }

        return false;
    }
}