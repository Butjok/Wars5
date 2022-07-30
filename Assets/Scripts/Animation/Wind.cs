using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Cloth))]
public class Wind : MonoBehaviour {
	public Cloth cloth;
	public Vector3 direction = Vector3.right;
	public Vector2 amplitude = new(1,5);
	public float frequency = 1;
	public void Start() {
		cloth = GetComponent<Cloth>();
		Assert.IsTrue(cloth);
	}
	public void Update() {
		cloth.externalAcceleration = direction * Mathf.Lerp(amplitude[0], amplitude[1], Mathf.Sin(Time.time*frequency)/2+.5f);
	}
}