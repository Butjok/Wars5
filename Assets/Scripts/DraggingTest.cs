using UnityEngine;

public class DraggingTest : MonoBehaviour {

    public Camera camera;
    public Transform target;

    private Vector3? oldMousePosition;
    public void Update() {

        /*    if (oldMousePosition is { } actualOldMousePosition) {
                var plane = new Plane(Vector3.up, Vector3.zero);
                var oldRay = camera.ScreenPointToRay(actualOldMousePosition);
                var newRay = camera.ScreenPointToRay(Input.mousePosition);
                plane.Raycast(oldRay, out var oldDistance);
                plane.Raycast(newRay, out var newDistance);
                var delta = oldRay.GetPoint(oldDistance) - newRay.GetPoint(newDistance);
                transform.position += delta;
                
            }
            
            oldMousePosition=Input.mousePosition;*/

        var plane = new Plane(Vector3.up, Vector3.zero);
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        plane.Raycast(ray, out var distance);
        var point = ray.GetPoint(distance);
        target.position = point;
    }


}