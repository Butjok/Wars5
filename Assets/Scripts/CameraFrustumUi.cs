using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraFrustumUi : MaskableGraphic {

    public Camera camera;
    public Plane groundPlane = new(Vector3.up, Vector3.zero);
    public Vector2 worldCenter;
    public Vector2 unitSize;

    // clockwise from bottom left
    public Vector2[] viewportPoints = { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };
    public Vector2[] worldHitPoints = new Vector2[4];
    public bool isValidFrustum = true;

    private void Update() {
        if (!camera)
            return;
        isValidFrustum = true;
        for (var i = 0; i < 4; i++) {
            var ray = camera.ViewportPointToRay(viewportPoints[i]);
            if (groundPlane.Raycast(ray, out var enter))
                worldHitPoints[i] = ray.GetPoint(enter).ToVector2();
            else {
                isValidFrustum = false;
                break;
            }
        }
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        if (isValidFrustum) {
            UIVertex V(int index) => new() {
                position = (worldHitPoints[index] - worldCenter) * unitSize,
                color = Color.white,
                uv0 = viewportPoints[index]
            };
            vertexHelper.AddQuad(V(0), V(1), V(2), V(3));
        }
    }
}