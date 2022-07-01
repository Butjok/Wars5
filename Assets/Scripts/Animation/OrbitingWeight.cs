using UnityEngine;

public class OrbitingWeight : MonoBehaviour {

	public Transform target;
	public float drag = 1.5f;
	public float force = 1000;
	public Vector3 velocity;
	public float maxDistance = .1f;

	[ContextMenu("Clear")]
	public void Start() {
		transform.position = target.position;
		velocity = Vector3.zero;
	}
	public void Update() {
		transform.position += velocity * Time.deltaTime;
		if (Vector3.Distance(target.position, transform.position) > maxDistance)
			transform.position = target.position + (transform.position - target.position).normalized * maxDistance;
		var to = target.position - transform.position;
		var force = to * this.force;
		force += -velocity * drag;
		velocity += force * Time.deltaTime;
	}
	private void OnDrawGizmosSelected() {
		Gizmos.DrawLine(target.position, transform.position);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(target.position, maxDistance);
	}
}