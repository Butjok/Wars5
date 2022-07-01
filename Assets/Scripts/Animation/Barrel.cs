using UnityEngine;
using UnityEngine.Assertions;

public class Barrel : MonoBehaviour {
	public BallisticMotion projectilePrefab;
	public BallisticComputer ballisticComputer;
	public Vector3 offset;
	public Transform forward;
	[ContextMenu(nameof(Shoot))]
	public void Shoot() {
		Assert.IsTrue(projectilePrefab);
		var position = transform.position + transform.TransformVector(offset);
		var projectile = Instantiate(projectilePrefab, position, transform.rotation);
		projectile.curve = BallisticCurve.From(position, forward.forward, ballisticComputer.velocity, ballisticComputer.gravity);
	}
	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position + transform.TransformVector(offset), .025f);
	}
}