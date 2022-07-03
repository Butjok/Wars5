using UnityEngine;

[ExecuteInEditMode]
public class ShadowQuality : MonoBehaviour {
	public float maxDistance = 10;
	public Vector2 rayPosition;
	public float multiplier = 1.5f;
	public void LateUpdate() {

		var ray = Camera.main.ViewportPointToRay(rayPosition);
		var plane = new Plane(Vector3.up, Vector3.zero);
		if (!plane.Raycast(ray, out var distance))
			distance = float.MaxValue;

		distance = Mathf.Min(maxDistance, distance * multiplier);
		QualitySettings.shadowDistance = distance;
	}
}