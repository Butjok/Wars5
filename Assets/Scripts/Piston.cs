using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Piston : MonoBehaviour {
	public Transform movingPart;
	public float defaultLength;
	public float velocity;
	public float springForce = 100;
	public float drag = 1.5f;
	public Vector2 clamp = new Vector2(-.25f, 1);
	public float constantForce;
	//public float length;
	public Quaternion startLocalRotation;
	public Vector3 startLocalPosition;
	public void Start() {
		startLocalRotation = Quaternion.Inverse(transform.rotation) * movingPart.rotation;
		startLocalPosition = transform.InverseTransformPoint(movingPart.position);
		defaultLength = Vector3.Distance(movingPart.position, transform.position);
	}
	public void Update() {
		var length = Vector3.Distance(transform.position, movingPart.position);

		length += velocity * Time.deltaTime;
		length = Mathf.Clamp(length, clamp[0], clamp[1]);
		var force = (defaultLength - length) * springForce;
		force -= velocity * drag;
		force += constantForce;
		velocity += force * Time.deltaTime;
		movingPart.transform.position = transform.TransformPoint(startLocalPosition.normalized * length);
		movingPart.transform.rotation = transform.rotation * startLocalRotation;

		if (constantForce != 0) 
			constantForce = 0;
	}
}