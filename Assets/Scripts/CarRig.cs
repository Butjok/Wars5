using UnityEngine;

[ExecuteInEditMode]
public class CarRig : MonoBehaviour {
	public Transform rayOrigin;
	public LayerMask mask;
	public float distance;
	public float rotationSmoothing = .1f;
	public void Update() {
		var ray = new Ray(rayOrigin.position, -rayOrigin.up);
		if (Physics.Raycast(ray, out var hit, float.MaxValue, mask)) {
			transform.position = hit.point + hit.normal * distance;
			var targetRotation=Quaternion.LookRotation(rayOrigin.forward,hit.normal);
			transform.rotation=Quaternion.Lerp(transform.rotation,targetRotation,Time.deltaTime*rotationSmoothing);
		}		
	}
}