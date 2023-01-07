using UnityEngine;

public class AABB : MonoBehaviour {
	public int x = 1;
	public Vector3 center;
	public Vector3 size;
	private void OnDrawGizmosSelected() {
		Gizmos.color=Color.yellow;
		
		Gizmos.DrawWireCube(transform.position, size);
	}
}