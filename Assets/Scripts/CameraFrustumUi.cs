using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraFrustumUi : MaskableGraphic {

    public Camera camera;
    public Plane groundPlane = new(Vector3.up, Vector3.zero);
    public Material material2;
    public Rect worldBounds;
    public Vector2 unitSize;

    public void ToggleEnabled() {
        enabled= !enabled;
    }

    // clockwise from bottom left
    public Vector2[] viewportPoints = { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };
    public Vector2[] worldHitPoints = new Vector2[4];
    public bool isValidFrustum = true;

    private void LateUpdate() {
        isValidFrustum = false;
        if (!camera || !material2)
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
        if (isValidFrustum) {
            material2.SetVector("_V0", worldHitPoints[0]);
            material2.SetVector("_V1", worldHitPoints[1]);
            material2.SetVector("_V2", worldHitPoints[2]);
            material2.SetVector("_V3", worldHitPoints[3]);
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        if (isValidFrustum) {
            var size = worldBounds.max - worldBounds.min;
            UIVertex V(int x, int y) => new() {
                color = Color.white,
                position = new Vector2Int(x, y) * size / 2 * unitSize,
                uv0 = new Vector2(x == -1 ? worldBounds.xMin : worldBounds.xMax, y == -1 ? worldBounds.yMin : worldBounds.yMax),
            };
            vertexHelper.AddQuad(V(-1, -1), V(-1, 1), V(1, 1), V(1, -1));
        }
    }
}