using System;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class ProjectileNew : MonoBehaviour {

	public float outgoingDistance;
	public Transform target;
	public Transform hitPoint;
	public float incomingDistance;
	public float incomingElevation;

	private void Update() {
		Draw.ingame.Line(transform.position, transform.position + transform.forward * outgoingDistance);
		var targetForward = new Vector3(target.forward.x, 0, target.forward.z);
		Assert.AreNotEqual(Vector3.zero, targetForward);
		var incoming = (targetForward.normalized * Mathf.Cos(incomingElevation) + Vector3.up * Mathf.Sin(incomingElevation)).normalized;
		Draw.ingame.Line(hitPoint.position, hitPoint.position + incoming * incomingDistance);
	}
}