using System;
using UnityEngine;

public class CameraFrustumProjection : MonoBehaviour {

	public Camera camera;
	public Transform planeTransform;

	private void OnDrawGizmos() {

		if (!camera || !planeTransform)
			return;

		var topLeftRay = camera.ViewportPointToRay(new Vector3(0, 1, 0));
		var topRightRay = camera.ViewportPointToRay(new Vector3(1, 1, 0));
		var bottomLeftRay = camera.ViewportPointToRay(new Vector3(0, 0, 0));
		var bottomRightRay = camera.ViewportPointToRay(new Vector3(1, 0, 0));

		var plane = new Plane(planeTransform.up, planeTransform.position);

		if (plane.Raycast(topLeftRay, out var topLeftEnter) &&
		    plane.Raycast(topRightRay, out var topRightEnter) &&
		    plane.Raycast(bottomLeftRay, out var bottomLeftEnter) &&
		    plane.Raycast(bottomRightRay, out var bottomRightEnter)) {

			var topLeftPoint = topLeftRay.GetPoint(topLeftEnter);
			var topRightPoint = topRightRay.GetPoint(topRightEnter);
			var bottomLeftPoint = bottomLeftRay.GetPoint(bottomLeftEnter);
			var bottomRightPoint = bottomRightRay.GetPoint(bottomRightEnter);

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(topLeftPoint, topRightPoint);
			Gizmos.DrawLine(topRightPoint, bottomRightPoint);
			Gizmos.DrawLine(bottomRightPoint, bottomLeftPoint);
			Gizmos.DrawLine(bottomLeftPoint, topLeftPoint);
		}
	}
}