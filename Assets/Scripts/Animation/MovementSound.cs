using UnityEngine;
using UnityEngine.Assertions;

public class MovementSound : MonoBehaviour {

	public AudioSource source;
	public Speedometer speedometer;
	public Accelerometer accelerometer;

	public void Update() {
		if (!speedometer) {
			speedometer = GetComponent<Speedometer>();
			Assert.IsTrue(speedometer);
		}
		if (!accelerometer) {
			accelerometer = GetComponent<Accelerometer>();
			Assert.IsTrue(accelerometer);
		}
	}
}