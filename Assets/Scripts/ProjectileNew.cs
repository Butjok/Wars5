using System;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class ProjectileNew : MonoBehaviour {

	public float outgoingDistance;
	public Transform hitPoint;
	public float incomingDistance;
	public float incomingElevation;

	private void Update() {
		Draw.ingame.Line(transform.position, transform.position + transform.forward * outgoingDistance);
		var targetForward = new Vector3(hitPoint.forward.x, 0, hitPoint.forward.z).normalized;
		var incoming = (targetForward * Mathf.Cos(incomingElevation) + Vector3.up * Mathf.Sin(incomingElevation)).normalized;
		Draw.ingame.Line(hitPoint.position, hitPoint.position + incoming * incomingDistance);
	}
}