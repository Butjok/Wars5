using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;

public class Accelerometer : MonoBehaviour {
	public Speedometer speedometer;
	public float? previousSpeed;
	public float acceleration;
	public void Update() {
		if (!speedometer) {
			speedometer = GetComponent<Speedometer>();
			Assert.IsTrue(speedometer);
		}
		if (previousSpeed is {} previous) {
			var delta = speedometer.speed - previous;
			acceleration = delta / Time.deltaTime;
		}
		previousSpeed = speedometer.speed;
	}
}