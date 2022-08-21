using UnityEngine;
using Random = UnityEngine.Random;

public class BuoyancyMovement : MonoBehaviour {
	public Vector3 amplitudes = new(3.5f, 3.5f, 1);
	public Vector3 frequency = new(3, 2.4567f, .2f);
	public Vector3 phases;
	public Vector3 values;
	public void Start() {
		phases = new Vector3(Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2);
	}
	public void Update() {
		var x = Mathf.Sin(Time.time * frequency.x + phases.x) * amplitudes.x;
		var y = Mathf.Sin(Time.time * frequency.y + phases.y) * amplitudes.y;
		var z = Mathf.Sin(Time.time * frequency.z + phases.z) * amplitudes.z;
		values = new Vector2(x, y);
		transform.localRotation = Quaternion.Euler(values.x, 0, values.y);
		transform.localPosition = new Vector3(0, z, 0);
	}
}