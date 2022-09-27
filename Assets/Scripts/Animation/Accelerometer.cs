using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Speedometer))]
public class Accelerometer : MonoBehaviour {
	public Speedometer speedometer;
	public double acceleration;
	public double[] speeds = {0,0,0};
	public float[] times = {0,0,0};
	public void Update() {
		if (!speedometer) {
			speedometer = GetComponent<Speedometer>();
			Assert.IsTrue(speedometer);
		}

		var deltaTime = times[0] - times[2];
		var deltaSpeed = speeds[0] - speeds[2];

		if (deltaTime > Mathf.Epsilon)
			acceleration = deltaSpeed / deltaTime;
		
		speeds[2] = speeds[1];
		speeds[1] = speeds[0];
		speeds[0] = speedometer.speed;

		times[2] = times[1];
		times[1] = times[0];
		times[0] = Time.time;
	}
}