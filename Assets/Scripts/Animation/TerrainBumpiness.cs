using UnityEngine;
using static UnityEngine.Mathf;

public class TerrainBumpiness : MonoBehaviour {
	public Vector2 scale = new Vector2(25, 25);
	public float amplitude = .005f;
	public static float Terrain(float t) {
		return Sin(t * 1) / 2 + .5f +
		       (Sin(t * 1.378222f + .25f) / 2 + .5f) / 2 +
		       (Sin(t * 3.43565f + 3.18f) / 2 + .5f) / 4 +
		       (Sin(t * 6.45574f + 56.18f) / 2 + .5f) / 8 +
		       (Sin(t * 7.43574f + 56.18f) / 2 + .5f) / 16;
	}
	public void Update() {
		var height = amplitude * Terrain(transform.position.x * scale.x) * Terrain(transform.position.z * scale.y);
		transform.position = new Vector3(transform.position.x, height, transform.position.z);
	}
}