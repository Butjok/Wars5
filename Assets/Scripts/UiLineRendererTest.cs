using UnityEngine;

public class UiLineRendererTest : MonoBehaviour {

    public UiLineRenderer lineRenderer;

    private void Update() {
        if (lineRenderer.PointsCount>100)
            lineRenderer.RemovePoint(0);
        lineRenderer.AddPoint(Input.mousePosition);
    }
}