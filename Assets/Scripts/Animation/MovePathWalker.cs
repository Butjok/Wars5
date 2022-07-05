using System;
using System.Collections.Generic;
using UnityEngine;

public class MovePathWalker : MonoBehaviour {

	public List<MovePath.Move> moves;
	public float time;
	public float speed = 1.5f;
	public event Action onComplete;

	public void Update() {
		if (moves is { Count: > 0 }) {
			var clamped = MovePath.Sample(moves, time, speed, out var position, out var direction, out var move);
			transform.position = position.ToVector3();
			transform.rotation = Quaternion.LookRotation(direction.ToVector3(), Vector3.up);
			if (clamped)
				enabled = false;
			time += Time.deltaTime;
		}
	}
	public void OnDisable() {
		time = float.MaxValue;
		Update();
		onComplete?.Invoke();
	}
	public void OnEnable() {
		time = 0;
	}

	private void OnDrawGizmosSelected() {
		if (moves == null)
			return;
		Gizmos.color = Color.blue;
		foreach (var m in moves) 
			Gizmos.DrawLine(m.start.ToVector3(), m.start.ToVector3() + ((Vector2)m.forward).ToVector3() / 2);
	}
}