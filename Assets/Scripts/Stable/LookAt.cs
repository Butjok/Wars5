using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour {
	public Transform target;
	public Transform relativeTo;
	public Vector3 localUp = Vector3.up;
	public void Update() {
		if (target) {
			var up = (relativeTo ? relativeTo : transform.parent).TransformDirection(localUp);
			transform.rotation = Quaternion.LookRotation(target.position - transform.position, up);
		}
	}
}