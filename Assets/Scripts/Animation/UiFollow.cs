using System;
using UnityEngine;
using UnityEngine.Assertions;

public class UiFollow : MonoBehaviour {
	public Transform target;
	public Camera camera;
	public void LateUpdate() {
		if (!target)
			return;
		if (!camera) {
			camera = Camera.main;
			Assert.IsTrue(camera);
		}
		var position = camera.WorldToScreenPoint(target.position);
		if (position.z < 0)
			transform.position = Vector3.one*float.MaxValue;
		else
			transform.position = position;
	}
}