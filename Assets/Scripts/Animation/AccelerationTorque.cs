using UnityEngine;
using UnityEngine.Assertions;

// TODO: fix for flipped

public class AccelerationTorque : MonoBehaviour {

	public BodyTorque bodyTorque;
	public Vector2? oldPosition;
	public double? oldSpeed;
	public double acceleration;
	public float factor = 100;
	public bool clamp;
	public float maxTorque;

	public void Update() {
		
		if (oldPosition is { } vector2) {
			var deltaPosition = transform.position.ToVector2() - vector2;
			var speed = (double)Vector2.Dot(transform.forward.ToVector2(), deltaPosition) / Time.deltaTime;
			if (oldSpeed is { } value)
				acceleration = (speed - value) / Time.deltaTime;
			oldSpeed = speed;
		}
		oldPosition = transform.position.ToVector2();

		var torque = (float)(acceleration * factor);
		if (clamp && !Mathf.Approximately(0, torque))
			torque = Mathf.Sign(torque) * Mathf.Min(maxTorque, Mathf.Abs(torque));

		bodyTorque.AddLocalTorque(new Vector3(torque, 0, 0));
	}
}