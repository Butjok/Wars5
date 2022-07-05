using UnityEngine;

public class UnitActionView : MonoBehaviour {
	public UnitAction action;
	public ChangeTracker<bool> selected;
	public void Awake() {
		selected = new ChangeTracker<bool>(_ => { });
	}
}