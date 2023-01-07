using UnityEngine;

public class CircleTest : MonoBehaviour {
	public Transform other;
	public float r1 = 1;
	public float r2 = .5f;
	public void OnDrawGizmos() {

		Vector2 i1, i2, end = default;

		var p1 = transform.position;
		var p2 = other.position;

		if (Maths.IntersectCircles(p1, r1, p2, r2, out i1, out i2)) {
			end = p2;
		}
		else {
			i1 = i2 = p1 + (p2 - p1).normalized * r1;
			if (Vector2.Distance(p1, p2) > r1 + r2) {
				end = p1 + (p2 - p1).normalized * (r1 + r2);
			}
			else {
				end = p1 + (p2 - p1).normalized * (r1 - r2);
			}
		}

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(p1, i1);
		Gizmos.DrawLine(i1, end);

		//Gizmos.color = Color.blue;
		//Gizmos.DrawLine(p1, i2);
		//Gizmos.DrawLine(i2, end);
	}
}