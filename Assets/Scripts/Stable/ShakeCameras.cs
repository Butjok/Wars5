using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class ShakeCameras : MonoBehaviour {

    public CinemachineImpulseSource impulseSource;
    public bool shakeOnAwake = true;
    public float force;

    private void Awake() {
        if (shakeOnAwake)
            Shake();
    }

    public void Shake() {
        if (impulseSource)
            impulseSource.GenerateImpulse(force);
    }
}