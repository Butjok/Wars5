using UnityEngine;

public class ManualControl:MonoBehaviour {
	public float speed;
	public float acceleration = 5;
	public float rotationSpeed = 90;
	public void Update() {
		speed += Input.GetAxisRaw("Vertical") * acceleration * Time.deltaTime;
		transform.rotation=Quaternion.Euler(0,transform.rotation.eulerAngles.y+Input.GetAxisRaw("Horizontal")*rotationSpeed*Time.deltaTime,0);
		transform.position += transform.forward * speed * Time.deltaTime;
	}
}