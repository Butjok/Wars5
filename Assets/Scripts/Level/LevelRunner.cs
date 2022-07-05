using UnityEditor;
using UnityEngine;

public class LevelRunner : MonoBehaviour {
	public Level level;
	public bool debugGui=true;
	public void Update() {
		level?.state.v?.Update();
	}
	public void OnGUI() {
		if (debugGui) {
			GUILayout.Label($"State: {level.state.v}");
		}
		level?.state.v?.DrawGUI();
	}
	public void OnDrawGizmos() {
		level?.state.v?.DrawGizmos();
		foreach (var (position,tile) in level.tiles) {
			Gizmos.DrawWireCube(position.ToVector3Int(),Vector2.one.ToVector3());
			Handles.Label(position.ToVector3Int(),tile.ToString());
		}
	}
	public void OnDrawGizmosSelected() {
		level?.state.v?.DrawGizmosSelected();
	}
}