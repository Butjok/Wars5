using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClampToHull : MonoBehaviour {

	public List<Vector2> points = new();
	public List<Vector2> Points {
		set => points = ConvexHull.ComputeConvexHull(value);
	}

	

	public void LateUpdate() {
		transform.position = ConvexHull.ClosestPoint(points, transform.position.ToVector2()).ToVector3();
	}

	
}