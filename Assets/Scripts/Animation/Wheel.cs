using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;

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
	public float constantSpinSpeed;
	public bool placeOnTerrain=true;

	[HideInInspector] public float spinAngle;

	[ContextMenu("Clear")]
	public void Start() {

		spinAngle = Random.Range(0, 360);

		if(!rayOrigin)
			rayOrigin = transform.parent;

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
	
	public Vector2 scale = new Vector2(25, 25);
	public float amplitude = .005f;
	public static float Terrain(float t) {
		return    Sin(t * 1) / 2 + .5f 
		       + (Sin(t * 1.378222f + .25f) / 2 + .5f) / 2 
		       + (Sin(t * 3.43565f + 3.18f) / 2 + .5f) / 4 
		       //+ (Sin(t * 6.45574f + 56.18f) / 2 + .5f) / 8 
		       //+ (Sin(t * 7.43574f + 56.18f) / 2 + .5f) / 16
		       ;
	}

	public void Update() {

		spinAngle += constantSpinSpeed;
		
		var radius = transform.TransformVector(Vector3.up * collider.radius).magnitude;
		if (speedometer.deltaPosition is { } delta) {
			var distance = Vector3.Dot(delta, rayOrigin.forward);
			const float ratio = 180 / Mathf.PI;
			var deltaAngle = distance / radius * ratio;
			spinAngle += deltaAngle;
		}

		var ray = new Ray(rayOrigin.position, Vector3.down);

		var noTilt = Quaternion.LookRotation(rayOrigin.forward, body ? body.transform.up : rayOrigin.up);
		
		if (placeOnTerrain) {
			if (Physics.SphereCast(ray, radius, out var hit, float.MaxValue, mask)) {
				Debug.DrawLine(transform.position, ray.GetPoint(hit.distance));
				transform.position = ray.GetPoint(hit.distance);

				var height = amplitude * Mathf.PerlinNoise(transform.position.x * scale.x, transform.position.z * scale.y);
				transform.position += height * Vector3.up;

				var plane = new Plane(-rayOrigin.up, rayOrigin.position);

				var normalAccumulator = Vector3.zero;
				var count = 1;
				for (var i = -2; i <= 2; i++) {
					var projected = plane.ClosestPointOnPlane(hit.point + i * rayOrigin.right / 20);
					var ray2 = new Ray(projected, -rayOrigin.up);
					if (i == 0 || !Physics.Raycast(ray2, out var hit2, float.MaxValue, mask))
						continue;
					normalAccumulator += hit2.normal;
					count++;
				}

				
				var fullTilt = Quaternion.LookRotation(rayOrigin.forward, normalAccumulator / count);
				var tilt = Quaternion.Lerp(noTilt, fullTilt, tiltInfluence);

				transform.rotation = tilt * Quaternion.Euler(spinAngle, 0, 0);
			}
		}
		else 
			transform.rotation=noTilt* Quaternion.Euler(spinAngle, 0, 0);
	}
}