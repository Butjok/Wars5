using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class DynamicShadowDistance : MonoBehaviour {
	
	public float maxDistance = 10;
	public Vector2 rayPosition;
	public float multiplier = 1.5f;
	public Camera camera;

	private void Awake() {
		Assert.IsTrue(camera);
	}
	private void LateUpdate() {

		if (!camera.isActiveAndEnabled)
			return;

		var ray = camera.ViewportPointToRay(rayPosition);
		var plane = new Plane(Vector3.up, Vector3.zero);
		if (!plane.Raycast(ray, out var distance))
			distance = maxDistance;
		else
			distance = Mathf.Min(maxDistance, distance * multiplier);
		
		QualitySettings.shadowDistance = distance;
	}
}