using UnityEngine;
using UnityEngine.Assertions;

public class AccelerationTorque : MonoBehaviour {
	
	public BodyTorque bodyTorque;
	public Vector2? oldPosition;
	public double? oldSpeed;
	public double acceleration;
	public float factor = 100;
	
	public void Update() {
		if (oldPosition is { } vector2) {
			var deltaPosition = transform.position.ToVector2() - vector2;
			var speed = (double)Vector2.Dot(transform.forward.ToVector2(), deltaPosition) / Time.deltaTime;
			if (oldSpeed is { } value)
				acceleration = (speed - value) / Time.deltaTime;
			oldSpeed = speed;
		}
		oldPosition = transform.position.ToVector2();
		
		bodyTorque.AddLocalTorque(new Vector3((float)(acceleration * factor),0,0));
	}
	public void OnGUI() {
		GUILayout.Label(acceleration.ToString("0.00"));
	}
}