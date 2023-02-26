using Drawing;
using UnityEngine;

public class Turret2 : MonoBehaviour {

    public Transform target;
    public Vector2 speeds = new(180, 60);
    public Transform barrel;
    public Vector2 barrelRestForward = new(1, 0);
    public Vector2 barrelPitchClamp = new(-45, 0);
    public float projectileVelocity = 5;
    public bool preferHighCurve = true;
    public Vector3 gravity = new Vector3(0, -5, 0);
    
    private void Update() {

        var hull = transform.parent;
        var turret = transform;

        // horizontal
        {
            Vector3 newForward;
            if (target) {
                var projectionPlane = new Plane(hull.up, turret.position);
                var targetPositionOnProjectionPlane = projectionPlane.ClosestPointOnPlane(target.position);
                newForward = targetPositionOnProjectionPlane - turret.position;
            }
            else
                newForward = hull.forward;

            if (newForward != Vector3.zero) {
                var targetRotation = Quaternion.LookRotation(newForward, hull.up);
                turret.rotation = turret.rotation.SlerpWithMaxSpeed(targetRotation, speeds[0]);
            }
        }

        // vertical
        if (barrel) {
            
            Vector3 newForward;
            if (target && BallisticCurve.TryCalculate(barrel.position, target.position, projectileVelocity, gravity, out var curveLow, out var curveHigh)) {

                var curve = preferHighCurve ? curveHigh : curveLow;
                var virtualTargetPosition = barrel.position + curve.forward * Mathf.Cos(curve.theta) + curve.up * Mathf.Sin(curve.theta);
                
                Draw.ingame.Cross(virtualTargetPosition);
                foreach (var (from,to) in curve.Segments())
                    Draw.ingame.Line(from,to);
                
                var projectionPlane = new Plane(turret.right, barrel.position);
                var targetPositionOnProjectionPlane = projectionPlane.ClosestPointOnPlane(virtualTargetPosition);
                newForward = targetPositionOnProjectionPlane - barrel.position;
            }
            else
                newForward = turret.forward * barrelRestForward[0] + turret.up * barrelRestForward[1];

            if (newForward != Vector3.zero) {
                var targetRotation = Quaternion.LookRotation(newForward, turret.up);
                barrel.rotation = barrel.rotation.SlerpWithMaxSpeed(targetRotation, speeds[1]);
                var pitchAngle = Vector3.SignedAngle(turret.forward, barrel.forward, turret.right);
                pitchAngle = Mathf.Clamp(pitchAngle, barrelPitchClamp[0], barrelPitchClamp[1]);
                barrel.localRotation = Quaternion.Euler(pitchAngle,0,0);
            }
        }
    }
}