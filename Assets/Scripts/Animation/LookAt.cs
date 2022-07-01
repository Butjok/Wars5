using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour {
	public Transform target;
	public Transform up;
	public void Update() {
		if (target && up)
			transform.rotation = Quaternion.LookRotation(target.position - transform.position, up.up);
		//Debug.DrawLine(transform.position,transform.position+target.position - transform.position);
	}
}