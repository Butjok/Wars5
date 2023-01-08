using UnityEngine;
using UnityEngine.Rendering;

public class Bird : MonoBehaviour {

    public enum Status { FlyingStraight, Steering }

    public float steeringDirection = 1;
    public Status status = Status.FlyingStraight;
    public Vector2 straightFlightTimeRange = new(1, 10);
    public Vector2 steeringTimeRange = new(1, 3);
    public float linearSpeed = 1;
    public float steeringSpeed = 90;
    public float nextTimestamp;
    public BoxCollider box;
    public MeshRenderer meshRenderer;
    public float visibilityDistance = 7.5f;

    public void Update() {

        var camera = Camera.main;
        meshRenderer.shadowCastingMode = camera && Vector3.Distance(camera.transform.position, transform.position) < visibilityDistance
            ? ShadowCastingMode.ShadowsOnly
            : ShadowCastingMode.On;

        transform.position += transform.forward * linearSpeed * Time.deltaTime;

        if (status == Status.Steering) {
            var euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, euler.y + steeringDirection * steeringSpeed * Time.deltaTime, euler.z);
        }

        if (box) {
            var bounds = box.bounds;
            var position = transform.position;

            if (transform.position.x > bounds.max.x)
                position.x = bounds.min.x;
            else if (transform.position.x < bounds.min.x)
                position.x = bounds.max.x;

            if (transform.position.z > bounds.max.z)
                position.z = bounds.min.z;
            else if (transform.position.z < bounds.min.z)
                position.z = bounds.max.z;

            transform.position = position;
        }

        if (Time.time >= nextTimestamp) {
            if (status == Status.FlyingStraight) {
                status = Status.Steering;
                steeringDirection = Random.value < .5f ? -1 : 1;
                nextTimestamp = Time.time + Random.Range(steeringTimeRange[0], steeringTimeRange[1]);
            }
            else if (status == Status.Steering) {
                status = Status.FlyingStraight;
                nextTimestamp = Time.time + Random.Range(straightFlightTimeRange[0], straightFlightTimeRange[1]);
            }
        }
    }
}