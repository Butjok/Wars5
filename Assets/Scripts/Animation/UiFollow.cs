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
			//position.y = Screen.height - position.y;
		transform.position = position;
	}
}