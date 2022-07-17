using System;
using UnityEngine;

[RequireComponent(typeof(Speedometer))]
public class TrackSpinner : MonoBehaviour {
	public Vector3 localForward = Vector3.forward;
	public Speedometer speedometer;
	public float speed = 1;
	public float offsetIntensity;
	public MeshRenderer renderer;
	public MaterialPropertyBlock materialPropertyBlock;
	public void Start() {
		speedometer = GetComponent<Speedometer>();
		renderer = GetComponent<MeshRenderer>();
		materialPropertyBlock = new MaterialPropertyBlock();
	}
	public void Update() {
		if (speedometer.deltaPosition is {} delta) {
			var forward = transform.TransformDirection(localForward);
			offsetIntensity +=Vector3.Dot(forward,delta)*speed;
			materialPropertyBlock.SetFloat("_OffsetIntensity",offsetIntensity);
			renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}