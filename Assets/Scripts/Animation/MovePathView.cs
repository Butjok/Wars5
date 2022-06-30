using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MovePathView : MonoBehaviour {
	public LineRenderer lineRenderer;
	public void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
	}
	
}