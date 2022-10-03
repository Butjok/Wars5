using UnityEngine;

public class HardFollowToggler : MonoBehaviour {
	public HardFollow hardFollow;
	public void Update() {
		if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace) && hardFollow)
			hardFollow.enabled = !hardFollow.enabled;
	}
}