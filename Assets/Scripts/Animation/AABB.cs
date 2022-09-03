using UnityEngine;

public class AABB : MonoBehaviour {
	public Vector3 center;
	public Vector3 size;
	private void OnDrawGizmosSelected() {
		Gizmos.color=Color.yellow;
		
		Gizmos.DrawWireCube(transform.position, size);
	}
}