using Butjok.CommandLine;
using UnityEngine;

public class ManualControl : MonoBehaviour {

    [Command] public  float acceleration = 5;
    [Command] public  float rotationSpeed = 90;
    [Command] public  float maxSpeed = 3;

    public float speed;
    public Transform target;

    public void Awake() {
        if (!target)
            target = transform;
    }
    
    public void Reset() {
        target = transform;
    }

    public void Update() {
        if (!target)
            return;
        speed += Input.GetAxisRaw("Debug Vertical") * acceleration * Time.deltaTime;
        speed = Mathf.Sign(speed) * Mathf.Min(maxSpeed, Mathf.Abs(speed));
        target.rotation = Quaternion.Euler(0, target.rotation.eulerAngles.y + Input.GetAxisRaw("Debug Horizontal") * rotationSpeed * Time.deltaTime, 0);
        target.position += target.forward * speed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            speed = 0;
    }
}