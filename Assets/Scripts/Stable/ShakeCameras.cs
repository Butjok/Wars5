using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ShakeCameras : MonoBehaviour {

    public Vector3 strength = new(.1f, .1f, .1f);
    public float duration = 1;
    public int vibrato = 10;
    public bool shakeOnAwake = true;

    private void Awake() {
        if (shakeOnAwake)
            Shake();
    }

    public void Shake() {
        foreach (var camera in FindObjectsOfType<Camera>().Where(camera => camera.isActiveAndEnabled)) {
            var distance = Vector3.Distance(transform.position, camera.transform.position);
            camera.transform.DOShakePosition(duration, strength / distance / distance, vibrato);
        }
    }
}