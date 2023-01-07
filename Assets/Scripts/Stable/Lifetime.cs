using UnityEngine;

public class Lifetime : MonoBehaviour {
	public float time = 5;
	public void Start() {
		Destroy(gameObject, time);
	}
}