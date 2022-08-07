using UnityEngine;

public class ManualControl:MonoBehaviour {
	public float speed;
	public float acceleration = 5;
	public float rotationSpeed = 90;
	public float maxSpeed = 3;
	public void Update() {
		speed += Input.GetAxisRaw("Debug Vertical") * acceleration * Time.deltaTime;
		speed = Mathf.Sign(speed) * Mathf.Min(maxSpeed,Mathf.Abs(speed));
		transform.rotation=Quaternion.Euler(0,transform.rotation.eulerAngles.y+Input.GetAxisRaw("Debug Horizontal")*rotationSpeed*Time.deltaTime,0);
		transform.position += transform.forward * speed * Time.deltaTime;
	}
}