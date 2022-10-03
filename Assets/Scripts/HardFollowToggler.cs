using UnityEngine;

public class HardFollowToggler : MonoBehaviour {
	public HardFollow hardFollow;
	public void Update() {
		if (Input.GetKeyDown(KeyCode.Backspace) && hardFollow)
			hardFollow.enabled = !hardFollow.enabled;
	}
}