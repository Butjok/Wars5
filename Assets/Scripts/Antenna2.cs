using Cinemachine.Utility;
using UnityEngine;

public class Antenna2 : MonoBehaviour {

    public Vector3 localTargetPosition;
    public Vector3 position;
    public float drag = 1.5f;
    public float force = 1000;
    public Vector3 velocity;
    public float maxDistance = .01f;
    public Transform target;

    public Vector3 WorldTargetPosition => transform.TransformPoint(localTargetPosition);

    private void Start() {
        position = WorldTargetPosition;
        velocity = Vector3.zero;
    }

    private void FixedUpdate() {

        if (position.IsNaN())
            position = WorldTargetPosition;
        if (velocity.IsNaN())
            velocity = Vector3.zero;

        position += velocity * Time.deltaTime;
        if (Vector3.Distance(position, WorldTargetPosition) > maxDistance)
            position = WorldTargetPosition + (position - WorldTargetPosition).normalized * maxDistance;
        var to = WorldTargetPosition - position;
        var force = to * this.force;
        force += -velocity * drag;
        velocity += force * Time.deltaTime;
    }

    private void Update() {
        if (target)
            target.rotation = Quaternion.LookRotation(position - transform.position, transform.parent.up);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(WorldTargetPosition, .05f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, .1f);
    }
}