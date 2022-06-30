using UnityEngine;

[ExecuteInEditMode]
public class OrbitingWeight : MonoBehaviour {

	public float drag = 1.5f;
	public float force = 1000;
	public Vector3 position;
	public Vector3 velocity;
	public float maxDistance = .1f;

	[ContextMenu("Clear")]
	public void Start() {
		position = transform.position;
		velocity = Vector3.zero;
	}
	public void Update() {
		position += velocity * Time.deltaTime;
		if (Vector3.Distance(transform.position, position) > maxDistance)
			position = transform.position + (position - transform.position).normalized * maxDistance;
		var to = transform.position - position;
		var force = to * this.force;
		force += -velocity * drag;
		velocity += force * Time.deltaTime;
	}
	private void OnDrawGizmosSelected() {
		Gizmos.DrawLine(transform.position, position);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, maxDistance);
	}
}