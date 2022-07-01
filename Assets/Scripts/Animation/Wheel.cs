using UnityEngine;
using UnityEngine.Assertions;

//[ExecuteInEditMode]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Speedometer))]
[SelectionBase]
public class Wheel : MonoBehaviour {

	public Body body;
	public Transform rayOrigin;
	public SphereCollider collider;
	public LayerMask mask;
	public Speedometer speedometer;
	[Range(0, 1)] public float tiltInfluence = .5f;
	public float spinAngle;
	public Accelerometer accelerometer;

	[ContextMenu("Clear")]
	public void Start() {

		Assert.IsTrue(rayOrigin);

		if (!collider) {
			collider = GetComponent<SphereCollider>();
			Assert.IsTrue(collider);
		}
		if (!speedometer) {
			speedometer = GetComponent<Speedometer>();
			Assert.IsTrue(speedometer);
		}
		
		if (!body) {
			var view = GetComponentInParent<UnitView>();
			if (view)
				body = view.GetComponentInChildren<Body>();
		}
	}

	public void Update() {

		var ray = new Ray(rayOrigin.position, -rayOrigin.up);
		if (Physics.SphereCast(ray, collider.radius, out var hit, float.MaxValue, mask)) {
			transform.position = ray.GetPoint(hit.distance);

			if (speedometer.deltaPosition is { } delta) {
				var distance = Vector3.Dot(delta, rayOrigin.forward);
				const float ratio = 180 / Mathf.PI;
				var deltaAngle = distance / collider.radius * ratio;
				spinAngle += deltaAngle;
			}

			var plane = new Plane(-rayOrigin.up, rayOrigin.position);

			var normal = Vector3.zero;
			var count = 1;
			for (var i = -2; i <= 2; i++) {
				var projected = plane.ClosestPointOnPlane(hit.point + i * rayOrigin.right / 20);
				var ray2 = new Ray(projected, -rayOrigin.up);
				if (i == 0 || !Physics.Raycast(ray2, out var hit2, float.MaxValue, mask))
					continue;
				normal += hit2.normal;
				count++;
			}

			var noTilt = Quaternion.LookRotation(rayOrigin.forward, body ? body.transform.up : rayOrigin.up);
			var fullTilt = Quaternion.LookRotation(rayOrigin.forward, normal / count);
			var tilt = Quaternion.Lerp(noTilt, fullTilt, tiltInfluence);

			transform.rotation = tilt * Quaternion.Euler(spinAngle, 0, 0);
		}
	}
}