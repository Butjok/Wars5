using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClampToHull : MonoBehaviour {

	public List<Vector2> hull = new();
	public bool rounded = true;
	public int circleSamples = 32;
	public float radius = 1;

	public LineRenderer lineRenderer;
	public PlaceOnTerrain placeOnTerrain;
	public float offset = .05f;

	[ContextMenu(nameof(Test))]
	public void Test() {
		var points = Enumerable.Range(0, 50).Select(_ => Random.insideUnitCircle * Random.value * 10).ToList();
		if (rounded) {
			var roundedPoints = new List<Vector2>();
			foreach (var point in points)
				for (var i = 0; i < circleSamples; i++) {
					var angle = 2 * Mathf.PI * ((float)i / circleSamples);
					var radius = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * this.radius;
					roundedPoints.Add(point + radius);
				}
			points = roundedPoints;
		}
		hull = ConvexHull.ComputeConvexHull(points);
	}
	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		for (var i = 0; i < hull.Count; i++)
			Gizmos.DrawLine(hull[i].ToVector3(), hull[(i + 1) % hull.Count].ToVector3());
	}
	private void LateUpdate() {
		var projected = transform.position.ToVector2();
		var clamped = ConvexHull.ClosestPoint(hull, projected);
		transform.position = clamped.ToVector3() + transform.position.y * Vector3.up;
	}

	public void Start() {

		lineRenderer = GetComponentInChildren<LineRenderer>();
		placeOnTerrain = GetComponent<PlaceOnTerrain>();

		if (!lineRenderer || hull.Count == 0)
			return;

		lineRenderer.positionCount = hull.Count; //+1;
		var positions = new List<Vector3>();
		foreach (var point2d in hull) {
			var point3d = placeOnTerrain && placeOnTerrain.Raycast(point2d, out var hit) ? hit.point : point2d.ToVector3();
			positions.Add(point3d + Vector3.up * offset);
		}
		//positions.Add(positions[0]);
		lineRenderer.SetPositions(positions.ToArray());
	}

	public void Recalculate(Game game) {
		var points = game.tiles.Keys.ToList();
		var roundedPoints = new List<Vector2>();
		foreach (var point in points)
			for (var i = 0; i < circleSamples; i++) {
				var angle = 2 * Mathf.PI * ((float)i / circleSamples);
				var radius = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * this.radius;
				roundedPoints.Add(point + radius);
			}
		hull = ConvexHull.ComputeConvexHull(roundedPoints);
	}
}