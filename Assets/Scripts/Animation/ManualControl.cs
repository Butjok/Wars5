using UnityEngine;

public class ManualControl:MonoBehaviour {
	public float speed;
	public float acceleration = 5;
	public float rotationSpeed = 90;
	public float maxSpeed = 3;
	public Transform target;
	public void Reset() {
		target = transform;
	}
	public void Update() {
		if (!target)
			return;
		speed += UnityEngine.Input.GetAxisRaw("Debug Vertical") * acceleration * Time.deltaTime;
		speed = Mathf.Sign(speed) * Mathf.Min(maxSpeed,Mathf.Abs(speed));
		target.rotation=Quaternion.Euler(0,target.rotation.eulerAngles.y+UnityEngine.Input.GetAxisRaw("Debug Horizontal")*rotationSpeed*Time.deltaTime,0);
		target.position += target.forward * speed * Time.deltaTime;
		if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
			speed = 0;
	}
}