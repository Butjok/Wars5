using UnityEngine;
using static UnityEngine.Mathf;

public class PlaceOnTerrain : MonoBehaviour {
	public LayerMask mask;
	public float speed = 1;
	public bool alwaysAbove = true;
	public const float rayOriginHeight = 100;
	public bool instant = false;
	public float offset = 0;

	public static bool TryRaycast(Vector2 position, out RaycastHit hit) {
		var rayOrigin = position.ToVector3() + Vector3.up * rayOriginHeight;
		return Physics.Raycast(rayOrigin, Vector3.down, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Terrain"));
	}

	public  bool TryRaycast(out RaycastHit hit) {
		return TryRaycast(transform.position.ToVector2(), out hit);
	}

	public void Start() {
		if (TryRaycast(out var hit))
			transform.position = hit.point + Vector3.up*offset;
	}

	public void LateUpdate() {
		if (!TryRaycast(out var hit))
			return;
		if (instant) {
			transform.position = hit.point + Vector3.up*offset;
			return;
		}
		var delta = hit.point.y + offset - transform.position.y;
		var maxDeltaThisFrame = Time.deltaTime * speed;
		if (Abs(delta) < maxDeltaThisFrame)
			transform.position = hit.point + Vector3.up*offset;
		else
			transform.position += Sign(delta) * maxDeltaThisFrame * Vector3.up;
		if (alwaysAbove && transform.position.y < hit.point.y)
			transform.position = hit.point + Vector3.up*offset;
	}
}