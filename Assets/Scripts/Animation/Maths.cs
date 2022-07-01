using UnityEngine;
using static UnityEngine.Mathf;

public static class Maths {
	
	public static bool IntersectCircles(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius,
		out Vector2 p1, out Vector2 p2) {

		float square(float x) => x * x;

		p1 = p2 = default;

		var distSq = (aPos - bPos).sqrMagnitude;
		var dist = Sqrt(distSq);
		var differentPosition = dist > 0.00001f;
		var maxRad = Max(aRadius, bRadius);
		var minRad = Min(aRadius, bRadius);
		var ringsTouching = Abs(dist - maxRad) < minRad;

		if (ringsTouching && differentPosition) {
			var aRadSq = aRadius * aRadius;
			var bRadSq = bRadius * bRadius;
			var lateralOffset = (distSq - bRadSq + aRadSq) / (2 * dist);
			var normalOffset = (0.5f / dist) * Sqrt(4 * distSq * aRadSq - square(distSq - bRadSq + aRadSq));
			var tangent = (bPos - aPos) / dist;
			Vector2 normal = new Vector2(-tangent.y, tangent.x); //tangent.Rotate90CCW();
			var chordCenter = aPos + tangent * lateralOffset;
			p1 = chordCenter + normal * normalOffset;
			p2 = chordCenter - normal * normalOffset;
			return true;
		}

		return false;
	}
}