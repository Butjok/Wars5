using UnityEngine;

public class RotateTo : MonoBehaviour {

	public Transform relativeTo;
	public Vector3 localRestingDirection = Vector3.forward;
	public Vector3 localNormal = Vector3.up;
	public Vector3 localUp = Vector3.up;

	public Transform target;
	public float duration90 = 1;

	public bool clamp;
	public Vector2 minmax = new Vector2(-45, 45);

	public void Update() {
		var up = relativeTo.TransformDirection(localUp);
		var normal = relativeTo.TransformDirection(localNormal);
		var plane = new Plane(normal, transform.position);
		var restingDirection = relativeTo.TransformDirection(this.localRestingDirection);
		var projected = target ? plane.ClosestPointOnPlane(target.position) : restingDirection;
		var targetAngle = Vector3.SignedAngle(restingDirection, projected - transform.position, normal);
		targetAngle = clamp ? Mathf.Clamp(targetAngle, minmax[0], minmax[1]) : targetAngle;
		var newProjected = Quaternion.AngleAxis(targetAngle, normal) * restingDirection;
		var targetRotation = Quaternion.LookRotation(newProjected, up);
		var angle = Quaternion.Angle(transform.rotation, targetRotation);
		var sectorDuration = duration90 * angle / 90;
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / sectorDuration);
	}
}