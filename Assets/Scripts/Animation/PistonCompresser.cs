using UnityEngine;
using UnityEngine.Serialization;

public class PistonCompresser : MonoBehaviour {

	public Transform centerOfMass;
	public Piston piston;
	[FormerlySerializedAs("torque")] public Vector3 worldTorque;

	public void Awake() {
		piston = GetComponent<Piston>();
	}

	private void Update() {
		var force = Vector3.Cross(worldTorque, transform.position - centerOfMass.position);
		var direction = piston.relativeTo.TransformDirection(piston.localDirection).normalized;
		piston.externalForce += Vector3.Dot(force, direction);
		worldTorque = Vector3.zero;
	}
}
