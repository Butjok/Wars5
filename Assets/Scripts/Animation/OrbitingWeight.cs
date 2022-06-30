using UnityEngine;

[ExecuteInEditMode]
public class OrbitingWeight : MonoBehaviour {

	public float drag = 1.5f;
	public float force = 1000;
	public Vector3 position;
	public Vector3 velocity;

	[ContextMenu("Clear")]
	public void Start() {
		position = transform.position;
		velocity = Vector3.zero;
	}
	public void Update() {
		position += velocity * Time.deltaTime;
		var to = transform.position - position;
		var force = to * this.force;
		force += -velocity * drag;
		velocity += force * Time.deltaTime;
	}
	private void OnDrawGizmosSelected() {
		Gizmos.DrawLine(transform.position, position);
	}
}