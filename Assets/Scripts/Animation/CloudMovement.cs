using UnityEngine;

public class CloudMovement : MonoBehaviour {
	public Vector3 velocity;
	public void Update() {
		transform.position += velocity * Time.deltaTime;
	}
}