using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class SideCamera : MonoBehaviour {

    public RenderTexture renderTexture;
    public Camera camera;
    public Vector2Int oldSize;

    public void Reset() {
        camera = GetComponent<Camera>();
        Assert.IsTrue(camera);
        Assert.IsTrue(renderTexture);
    }

    public void Update() {
        var size = new Vector2Int(camera.pixelWidth, camera.pixelHeight);
        if (oldSize != size) {
            oldSize = size;
            renderTexture.width = size.x;
            renderTexture.height = size.y;
        }
    }
}