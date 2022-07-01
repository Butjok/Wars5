using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathWalker : MonoBehaviour {

	public float rotation90Time = .15f;
	public float speed = 5;
	public ChangeTracker<float> time;

	public Color directionColor = Color.blue;
	public float directionLength = 1;
	public Color tileColor = new(1, 1, 1, .5f);

	public ChangeTracker<IEnumerable<Vector2>> points;
	public event Action onComplete;

	public MovePath path;
	public bool walking;

	private void Awake() {

		points = new ChangeTracker<IEnumerable<Vector2>>(_ => path = new MovePath(points.v));

		time = new ChangeTracker<float>(_ => {

			if (path == null || !path.Sample(this.speed, rotation90Time, time.v, out var position, out var direction))
				return;

			transform.position = position.ToVector3();
			transform.rotation = Quaternion.LookRotation(direction.ToVector3());
		});
	}

	public void OnDrawGizmos() {
		if (path == null || path.IsEmpty)
			return;
		Gizmos.color = tileColor;
		foreach (var tile in path.Tiles) {
			Gizmos.DrawWireCube(((Vector2)tile).ToVector3(), Vector2.one.ToVector3());
		}

		Gizmos.color = directionColor;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * directionLength);
	}

	public void Walk() {
		time.v = 0;
		walking = true;
	}

	public void Update() {
		if (!walking)
			return;
		if (time.v >= path.Duration(speed, rotation90Time)) {
			walking = false;
			onComplete?.Invoke();
		}
		else
			time.v += Time.deltaTime;
	}
}