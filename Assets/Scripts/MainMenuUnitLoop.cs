using UnityEngine;

public class MainMenuUnitLoop : MonoBehaviour {

    public float length = 1;
    public Transform[] actors = { };
    public float speed = 1;
    public bool rotate = true;

    public Vector3 LineStart => transform.position;
    public Vector3 LineEnd => transform.position + LineDirection * length;
    public Vector3 LineDirection => transform.forward;

    public static Vector3 GetClosestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
        var lineDirection = lineEnd - lineStart;
        var lineMagnitude = lineDirection.magnitude;
        lineDirection.Normalize();

        var closestPointDistance = Vector3.Dot((point - lineStart), lineDirection);
        closestPointDistance = Mathf.Clamp(closestPointDistance, 0f, lineMagnitude);

        return lineStart + lineDirection * closestPointDistance;
    }

    public void Update() {
        foreach (var actor in actors) {
            var closestPoint = GetClosestPointOnLineSegment(LineStart, LineEnd, actor.position);
            var distance = (closestPoint - LineStart).magnitude;
            var nextDistance = distance + speed * Time.deltaTime;
            var oldPosition = actor.position;
            var nextPosition = LineStart + LineDirection * nextDistance;
            if (rotate && nextPosition != oldPosition)
                actor.rotation = Quaternion.LookRotation(nextPosition - oldPosition, transform.up);
            var wrappedDistance = nextDistance % length;
            actor.position = LineStart + LineDirection * wrappedDistance;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(LineStart, LineEnd);
    }
}