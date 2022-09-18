using UnityEngine;

public class ExplosionTracer : MonoBehaviour {
	public LayerMask layerMask;
	public ParticleSystem explosion;
	public float power = 500;
	public void Update() {
		
		if (Camera.main && Input.GetKeyDown(KeyCode.KeypadDivide)) {
			
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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