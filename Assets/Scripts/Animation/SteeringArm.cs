using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Speedometer))]
public class SteeringArm : MonoBehaviour {
	public float friction = 1;
	public float duration90 = .25f;
	public Speedometer speedometer;
	public SteeringArm[] neighbors = { };
	public void Update() {
		if (DeltaPosition is not { } delta)
			return;
		var count = 1;
		foreach (var neighbor in neighbors)
			if (neighbor.DeltaPosition is { } neighborDeltaPosition) {
				delta += neighborDeltaPosition;
				count++;
			}
		var target = Quaternion.LookRotation(delta / count, transform.parent.up);
		var sectorAngle = Quaternion.Angle(transform.rotation, target);
		var sectorDuration = duration90 * sectorAngle / (90);
		transform.rotation = Quaternion.Lerp(transform.rotation, target, friction * (Time.deltaTime / sectorDuration) * (delta.magnitude / Time.deltaTime));
	}
	public Vector3? DeltaPosition {
		get {
			if (!speedometer) {
				speedometer = GetComponent<Speedometer>();
				Assert.IsTrue(speedometer);
			}
			if (speedometer.deltaPosition is not { } deltaPosition || deltaPosition == Vector3.zero)
				return null;
			return deltaPosition;
		}
	}
	[ContextMenu(nameof(FindNeighbours))]
	public void FindNeighbours() {
		neighbors = GetComponentInParent<UnitView>().GetComponentsInChildren<SteeringArm>().Except(new[] { this }).ToArray();
	}
}