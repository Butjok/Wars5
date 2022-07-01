using UnityEngine;

[ExecuteInEditMode]
public class Piston : MonoBehaviour {

	public Transform relativeTo;
	public Vector3 direction = Vector3.forward;

	public float targetLength = .1f;
	public Vector2 clamp = new(.5f, 1);
	public float force = 250;
	public float drag = 4;
	public float constantForce;

	[HideInInspector] public float velocity;
	[HideInInspector] public Vector3 position;

	public void Update() {

		var direction = (relativeTo ? relativeTo : transform).TransformDirection(this.direction).normalized;
		var length = Vector3.Dot(direction, position - transform.position);

		var force = (targetLength - length) * this.force;
		force -= velocity * drag;
		force += constantForce;
		velocity += force * Time.deltaTime;

		length += velocity * Time.deltaTime;
		length = Mathf.Clamp(length, clamp[0], clamp[1]);

		position = transform.position + direction * length;
	}

	[ContextMenu(nameof(Clear))]
	public void Clear() {
		velocity = 0;
		var direction = (relativeTo ? relativeTo : transform).TransformDirection(this.direction).normalized;
		position = transform.position + direction * targetLength;
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(transform.position, transform.position + (relativeTo ? relativeTo : transform).TransformDirection(this.direction).normalized * targetLength);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(position, .01f);
	}
}