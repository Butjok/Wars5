using UnityEngine;

[ExecuteInEditMode]
public class Turret : MonoBehaviour {
	public Transform target;
	public Transform barrel;
	public float turretDuration90 = 1;
	public float barrelDuration90 = 1;
	public Vector2 barrelAngleClamp = new Vector2(0, 45);
	public bool restTurret;
	public bool restBarrel;

	public void Update() {
		if (target) {
			{
				var plane = new Plane(transform.parent.up, transform.position);
				var projected = plane.ClosestPointOnPlane(target.position);
				var direction = restTurret ? transform.parent.forward : (projected - transform.position);
				var angle = Vector3.SignedAngle(transform.forward, direction, transform.parent.up);
				var sectorDuration = Mathf.Abs(angle) / 90 * turretDuration90;
				var targetRotation = Quaternion.LookRotation(direction, transform.parent.up);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / sectorDuration);
			}

			if (barrel) {
				var plane2 = new Plane(transform.right, barrel.position);
				var projected2 = plane2.ClosestPointOnPlane(target.position);
				var direction2 = restBarrel ? transform.forward : (projected2 - barrel.position);
				var angle = Vector3.SignedAngle(transform.forward, direction2, transform.right);
				angle = Mathf.Clamp(angle, barrelAngleClamp[0], barrelAngleClamp[1]);
				var targetRotation = Quaternion.Euler(angle, 0, 0);
				var sector = Quaternion.Angle(barrel.localRotation, targetRotation);
				var sectorDuration = sector / 90 * barrelDuration90;
				barrel.localRotation = Quaternion.Lerp(barrel.localRotation, targetRotation, Time.deltaTime / sectorDuration);
			}
		}
	}
}