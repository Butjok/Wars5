using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class DynamicShadowDistanceSetter : MonoBehaviour {

    [Command] public static Vector2 rayPosition = new(0, 1);
    [Command] public float maxDistance = 45;
    [Command] public float multiplier = 1.5f;

    public Camera camera;

    private void Reset() {
        camera = Camera.main;
    }
    private void Awake() {
        Assert.IsTrue(camera);
    }
    private void LateUpdate() {

        if (!camera.isActiveAndEnabled)
            return;

        var ray = camera.ViewportPointToRay(rayPosition);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out var distance))
            distance = maxDistance;
        else
            distance = Mathf.Min(maxDistance, distance * multiplier);

        QualitySettings.shadowDistance = distance;
    }
}