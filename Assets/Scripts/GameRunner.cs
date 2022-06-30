using System;
using UnityEngine;

public class GameRunner : MonoBehaviour {
	public Game game;
	public bool debugGui=true;
	public void Update() {
		game?.state.v?.Update();
	}
	public void OnGUI() {
		if (debugGui) {
			GUILayout.Label($"State: {game.state.v}");
		}
		game?.state.v?.DrawGUI();
	}
	public void OnDrawGizmos() {
		game?.state.v?.DrawGizmos();
	}
	public void OnDrawGizmosSelected() {
		game?.state.v?.DrawGizmosSelected();
	}
}