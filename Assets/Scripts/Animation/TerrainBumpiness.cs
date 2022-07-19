using UnityEngine;
using static UnityEngine.Mathf;

public class TerrainBumpiness : MonoBehaviour {
	public Vector2 scale = new Vector2(25, 25);
	public float amplitude = .005f;
	public void Update() {
		var height = amplitude * Mathf.PerlinNoise(transform.position.x * scale.x, transform.position.z * scale.y);
		transform.position = new Vector3(transform.position.x, height, transform.position.z);
	}
}