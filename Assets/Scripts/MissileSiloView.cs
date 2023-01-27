using System;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

public class MissileSiloView : MonoBehaviour {

    public Transform target;
    public Vector3 targetPosition;
    public Transform barrel;
    public Vector2 speeds = new Vector2(180, 90);
    public Vector2 angleBounds = new Vector2(0, 80);
    public float minVelocity = 3;
    public float velocityStep = .1f;
    public float gravity = 5;

    public Renderer[] renderers = Array.Empty<Renderer>();

    public MaterialPropertyBlock materialPropertyBlock;
    private void Awake() {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    public Color PlayerColor {
        set {
            materialPropertyBlock.SetColor("_PlayerColor", value);
            foreach (var renderer in renderers)
                renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }

    public BallisticMotion projectilePrefab;
    public bool selectHighCurve = true;

    private void Update() {

        if (target)
            targetPosition = target.position;

        // rotate
        {
            var groundPlane = new Plane(transform.up, transform.position);
            var targetPositionOnGroundPlane = groundPlane.ClosestPointOnPlane(targetPosition);
            var newForward = targetPositionOnGroundPlane - transform.position;
            if (newForward != Vector3.zero)
                transform.rotation = transform.rotation.SlerpWithMaxSpeed(Quaternion.LookRotation(newForward, transform.up), speeds[0]);

            // var sidePlane = new Plane(transform.right, transform.position);
            // var targetPositionOnSidePlane = sidePlane.ClosestPointOnPlane(targetPosition);

            var stepsTaken = 0;
            for (var velocity = minVelocity; ; velocity += velocityStep, stepsTaken++) {

                Assert.IsTrue(stepsTaken < 10000, "too many steps taken to calculate the minimum projectile velocity");

                if (!BallisticCurve.TryCalculate(transform.position, targetPosition, velocity, Vector3.down * gravity, out var curveLow, out var curveHigh))
                    continue;

                var curve = selectHighCurve ? curveHigh : curveLow;

                // hack to being able to fire straight up
                if (curve.totalTime is { } totalTime && Mathf.Approximately(0, totalTime) && Mathf.Approximately(curve.theta, Mathf.PI / 2))
                    curve.totalTime = 2 * velocity / gravity;

                foreach (var (start, end) in curve.Segments())
                    Draw.ingame.Line(start, end);

                Draw.Label2D(targetPosition, curve.totalTime.ToString());

                var target = transform.position + curve.forward * Mathf.Cos(curve.theta) + curve.up * Mathf.Sin(curve.theta);
                var angle = Vector3.SignedAngle(transform.forward, target, transform.right);

                var clampedAngle = Mathf.Clamp(angle, angleBounds[0], angleBounds[1]);
                barrel.localRotation = barrel.localRotation.SlerpWithMaxSpeed(Quaternion.Euler(clampedAngle, 0, 0), speeds[1]);

                if (Input.GetKeyDown(KeyCode.Space)) {
                    var projectile = Instantiate(projectilePrefab, transform.position, barrel.rotation);
                    projectile.time = 0;
                    projectile.curve = curve;
                }

                break;
            }
        }
    }
}