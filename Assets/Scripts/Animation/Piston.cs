using UnityEngine;

[ExecuteInEditMode]
public class Piston : MonoBehaviour {

    public Transform relativeTo;
    public Vector3 localDirection = Vector3.up;

    public float targetLength = 0;
    public Vector2 clamp = new(-0.025f, 0.025f);
    public float force = 250;
    public float drag = 4;
    public float forceThisFrame;

    public float velocity;
    public Vector3 position;

    public void Start() {
        Reset();
    }
    
    public void Update() {

        var direction = (relativeTo ? relativeTo : transform).TransformDirection(localDirection).normalized;
        var length = Vector3.Dot(direction, position - transform.position);

        var force = (targetLength - length) * this.force;
        force -= velocity * drag;
        force += forceThisFrame;
        velocity += force * Time.deltaTime;
        
        length += velocity * Time.deltaTime;
        length = Mathf.Clamp(length, clamp[0], clamp[1]);

        position = transform.position + direction * length;

        forceThisFrame = 0;
    }

    [ContextMenu(nameof(Reset))]
    public void Reset() {
        velocity = 0;
        var direction = (relativeTo ? relativeTo : transform).TransformDirection(localDirection).normalized;
        position = transform.position + direction * targetLength;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (relativeTo ? relativeTo : transform).TransformDirection(localDirection).normalized * targetLength);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(position, .05f);
    }
}