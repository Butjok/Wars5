using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Speedometer))]
public class BuoyancyMovement : MonoBehaviour {
	public Vector3 amplitudes = new(3.5f, 3.5f, 1);
	public Vector3 frequency = new(3, 2.4567f, .2f);
	public Vector3 phases;
	public Vector3 values;
	public Speedometer speedometer;
	public float maxSpeed = 5;
	public float speedMultiplier = 2;
	public Vector3 time;
	public void Reset() {
		speedometer = GetComponent<Speedometer>();
		Assert.IsTrue(speedometer);
	}
	public void Start() {
		phases = new Vector3(Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2, Random.value * Mathf.PI * 2);
	}
	public void Update() {
		var multiplier = speedometer.speed is var speed ? Mathf.Lerp(1, speedMultiplier, speed / maxSpeed) : 1;

		time.x += Time.deltaTime * frequency.x * multiplier;
		time.y += Time.deltaTime * frequency.y * multiplier;
		time.z += Time.deltaTime * frequency.z * multiplier;

		var x = Mathf.Sin(time.x + phases.x) * amplitudes.x;
		var y = Mathf.Sin(time.y + phases.y) * amplitudes.y;
		var z = Mathf.Sin(time.z + phases.z) * amplitudes.z;

		values = new Vector2(x, y);

		transform.localRotation = Quaternion.Euler(values.x, 0, values.y);
		transform.localPosition = new Vector3(0, z, 0);
	}
}