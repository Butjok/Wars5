using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class Trail:MonoBehaviour {
	public LineRenderer trailRenderer;
	public Vector3? lastPosition;
	public void Start() {
		trailRenderer = GetComponent<LineRenderer>();
		trailRenderer.SetPositions(new Vector3[2]);
	}
	public void Update() {
		if (lastPosition is { } position) {
			trailRenderer.SetPosition(0,transform.position);
			trailRenderer.SetPosition(1,position);
		}
		lastPosition = transform.position;
	}
}
