using UnityEngine;
using static UnityEngine.Mathf;

public class Rotator : MonoBehaviour {

	public Transform relativeTo;
	public Vector3 localNormal = Vector3.up;
	public Vector3 localForward = Vector3.forward;

	[Header("From->To")]
	public Transform from;
	public Transform target;

	[Header("Angle Limits")]
	public bool doClamp;
	public Vector2 clamp = new(-180, 180);

	[Header("Spring")]
	public float force = 1000;
	public float drag = 25;
	public float velocity;
	public float maxVelocity = 180;
	
	[Space]
	public float angle;

	public void Update() {

		if (!from || !target)
			return;
		
		var normal = relativeTo.rotation * localNormal;
		var forward = relativeTo.rotation * Quaternion.AngleAxis(angle, localNormal) * localForward;
		var plane = new Plane(normal, from.position);
		var projected = plane.ClosestPointOnPlane(target.position);

		var deltaAngle = Vector3.SignedAngle(forward, projected - from.position, normal);
		var force = deltaAngle * this.force;
		force -= velocity * drag;
		velocity += force * Time.deltaTime;
		if (Abs(velocity) > maxVelocity)
			velocity = Sign(velocity) * maxVelocity;
		angle += velocity * Time.deltaTime;
		if (doClamp)
			angle = Clamp(angle, clamp[0], clamp[1]);

		transform.rotation = relativeTo.rotation * Quaternion.AngleAxis(angle, localNormal);
	}
}