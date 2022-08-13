using System;
using UnityEngine;

[RequireComponent(typeof(Speedometer))]
public class Stepping : MonoBehaviour {
	public Speedometer speedometer;
	public float distance;
	public float step;
	public float left, right;
	public void Start() {
		speedometer = GetComponent<Speedometer>();
	}
	public void Update() {
		if (speedometer.deltaPosition is { } delta)
			distance += Vector3.Dot(transform.forward,delta);
		left = Mathf.Floor(distance / step) * step;
		right = Mathf.Round(distance / step) * step;
	}
	public void OnDrawGizmos() {
		Gizmos.color=Color.yellow;
		Gizmos.DrawWireSphere(transform.position+transform.forward*left,.25f);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.position+transform.forward*right,.25f);
	}
}