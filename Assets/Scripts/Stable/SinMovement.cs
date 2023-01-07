using UnityEngine;

public class SinMovement : MonoBehaviour {
	public Vector3 from, to;
	public float timeMultiplier=1;
	public float phaseShift = 0;
	public void Update() {
		transform.position=Vector3.Lerp(@from,to,.5f+Mathf.Sin(Time.time*timeMultiplier)/2);
	}
}