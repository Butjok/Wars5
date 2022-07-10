using UnityEngine;
using static UnityEngine.Mathf;

public class PlaceOnTerrain : MonoBehaviour {
	public LayerMask mask;
	public float speed = 1;
	public bool alwaysAbove = true;
	public float rayOriginHeight = 100;
	public void LateUpdate() {
		var position = transform.position.ToVector2();
		var rayOrigin = position.ToVector3() + Vector3.up * rayOriginHeight;
		if (!Physics.Raycast(rayOrigin, Vector3.down, out var hit, float.MaxValue, mask))
			return;
		var offset = hit.point.y - transform.position.y;
		var maxOffsetThisFrame = Time.deltaTime * speed;
		if (Abs(offset) < maxOffsetThisFrame)
			transform.position = hit.point;
		else
			transform.position += Sign(offset) * maxOffsetThisFrame * Vector3.up;
		if (alwaysAbove && transform.position.y < hit.point.y)
			transform.position = hit.point;
	}
}