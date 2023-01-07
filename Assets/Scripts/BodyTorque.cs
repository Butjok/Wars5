using System;
using UnityEngine;

public class BodyTorque : MonoBehaviour {

	public PistonCompresser[] compressers = Array.Empty<PistonCompresser>();

	[ContextMenu(nameof(Awake))]
	public void Awake() {
		var unitView = GetComponentInParent<UnitView>();
		var parent = unitView ? unitView.transform : transform.parent;
		compressers = parent.GetComponentsInChildren<PistonCompresser>();
	}

	public void AddLocalTorque(Vector3 localTorque) {
		var worldTorque = transform.TransformDirection(localTorque);
		foreach (var compresser in compressers)
			compresser.worldTorque += worldTorque;
	}

	public void AddWorldForceTorque(Vector3 worldPosition, Vector3 worldForce) {
		var worldTorque = Vector3.Cross(worldPosition - transform.position, worldForce);
		var localTorque = transform.InverseTransformDirection(worldTorque);
		AddLocalTorque(localTorque);
	}
}