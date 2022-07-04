using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyTorque : MonoBehaviour {

	public PistonCompresser[] compressers = Array.Empty<PistonCompresser>();
	private Vector3 localTorque;
	private Vector3 localRecoilTorque;
	public Vector3 accelerationTorque;
	public Transform centerOfMass;
	public Transform barrel;
	public float recoilForce = 100;

	[ContextMenu(nameof(Awake))]
	public void Awake() {
		var unitView = GetComponentInParent<UnitView>();
		var parent = unitView ? unitView.transform : transform.parent;
		compressers = parent.GetComponentsInChildren<PistonCompresser>();
	}
	public void Update() {
		if(Input.GetKeyDown(KeyCode.Insert))
			RecoilTorque(barrel.position, barrel.forward * recoilForce);

		if(Input.GetKeyDown(KeyCode.Home))
			localTorque = accelerationTorque;
		if(Input.GetKeyUp(KeyCode.Home))
			localTorque = Vector3.zero;

		if(Input.GetKeyDown(KeyCode.End))
			localTorque = -accelerationTorque;
		if(Input.GetKeyUp(KeyCode.End))
			localTorque = Vector3.zero;

		var worldTorque = transform.TransformDirection(localTorque);
		foreach (var compresser in compressers)
			compresser.worldTorque = worldTorque;
	}

	public void RecoilTorque(Vector3 worldPosition, Vector3 worldForce) {
		var worldTorque = Vector3.Cross(worldPosition - transform.position, worldForce);
		localRecoilTorque = transform.InverseTransformDirection(worldTorque);
		StartCoroutine(RecoilTorqueAnimation);
	}

	public IEnumerator RecoilTorqueAnimation {
		get {
			localTorque = localRecoilTorque;
			yield return null;
			localTorque = Vector3.zero;
		}
	}
}