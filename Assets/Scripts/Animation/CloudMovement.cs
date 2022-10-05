using UnityEngine;

public class CloudMovement : MonoBehaviour {

	public static CloudMovement instance;
	
	public Vector3 velocity;

	public void Awake() {
		instance = this;
	}
	
	public void Update() {
		transform.position += velocity * Time.deltaTime;
	}
}