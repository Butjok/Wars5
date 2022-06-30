using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour {
	public OrbitingWeight weight;
	public Transform up;
	public void Update() {
		if (weight && up)
			transform.rotation = quaternion.LookRotation(weight.position - transform.position, up.up);
	}
}