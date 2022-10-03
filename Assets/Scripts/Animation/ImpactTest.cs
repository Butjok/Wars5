using UnityEngine;

public class ImpactTest : MonoBehaviour {
	public ParticleSystem[] particleSystems;
	public ParticleSystem ps;
	public void Awake() {
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}
	public void Update() {
		if (Input.GetKeyDown(KeyCode.Insert)) {
			if(ps)
				ps.Play();
			//foreach (var particleSystem in particleSystems)
//				particleSystem.Play();
		}
	}
}