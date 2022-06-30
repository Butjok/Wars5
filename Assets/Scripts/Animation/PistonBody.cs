using UnityEngine;

[ExecuteInEditMode]
public class PistonBody : MonoBehaviour {
	public Piston piston;
	public void Update() {
		if (piston)
			transform.position = piston.position;
	}	
}