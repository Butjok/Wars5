using System;
using System.Linq;
using UnityEngine;

public class ExplosionTracer : MonoBehaviour {
	
	public LayerMask layerMask;
	public ParticleSystem explosion;
	public float power = 500;
	public Camera[] cameras = Array.Empty<Camera>();
	
	public void Update() {
		
		var mousePosition = Input.mousePosition / new Vector2(Screen.width,Screen.height);
		var actualCamera = cameras.FirstOrDefault(c => c.isActiveAndEnabled && c.rect.Contains(mousePosition));
		if (!actualCamera)
			actualCamera = Camera.main;

		if (actualCamera && Input.GetKeyDown(KeyCode.KeypadDivide)) {
			
			var ray = actualCamera.ScreenPointToRay((Vector2)Input.mousePosition - actualCamera.rect.min);
			if (Physics.Raycast(ray, out var hit, float.MaxValue, layerMask)) {
				
				Debug.DrawLine(hit.point, hit.point + hit.normal, Color.blue, 1);
				
				explosion.transform.position = hit.point;
				explosion.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
				explosion.Play();

				var bodyTorque = hit.collider.GetComponentInParent<BodyTorque>();
				if (bodyTorque)
					bodyTorque.RecoilTorque(hit.point, -hit.normal * power);
			}
		}
	}
}