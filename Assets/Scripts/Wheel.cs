using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Speedometer))]
public class Wheel : MonoBehaviour {
	public Transform rayOrigin;
	public SphereCollider collider;
	public LayerMask mask;
	public Speedometer speedometer;
	[FormerlySerializedAs("wheelTilt"), Range(0, 1)]
	public float tiltInfluence = .5f;
	[FormerlySerializedAs("angle")] public float spinAngle;


	public void Update() {
		Assert.IsTrue(rayOrigin);
		if (!collider) {
			collider = GetComponent<SphereCollider>();
			Assert.IsTrue(collider);
		}
		if (!speedometer) {
			speedometer = GetComponent<Speedometer>();
			Assert.IsTrue(speedometer);
		}
		var ray = new Ray(rayOrigin.position, -rayOrigin.up);
		if (Physics.SphereCast(ray, collider.radius, out var hit, float.MaxValue, mask)) {
			transform.position = ray.GetPoint(hit.distance);

			if (speedometer.deltaPosition is {} delta) {
				var distance = Vector3.Dot(delta, rayOrigin.forward);
				const float ratio = 180 / Mathf.PI;
				var deltaAngle = distance / collider.radius * ratio;
				spinAngle += deltaAngle;
			}
			//Debug.DrawRay(transform);

			var plane = new Plane(-rayOrigin.up, rayOrigin.position);
			
			var normal = Vector3.zero;
			var count = 1;
			for (var i = -2; i <= 2; i++) {
				var projected = plane.ClosestPointOnPlane(hit.point + i*rayOrigin.right/20);
				var ray2 = new Ray(projected, -rayOrigin.up);
				if (i == 0 || !Physics.Raycast(ray2, out var hit2, float.MaxValue, mask)) 
					continue;
				normal += hit2.normal;
				count++;
			}
			
			var fullTilt = Quaternion.LookRotation(rayOrigin.forward, normal/count);
			var tilt = fullTilt;
			
			transform.rotation = tilt * Quaternion.Euler(spinAngle, 0, 0);

		}
	}
}