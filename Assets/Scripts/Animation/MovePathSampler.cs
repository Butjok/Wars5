using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MovePathSampler {
	
	public static Queue<MovePath.Move> queue = new();

	public static bool Sample(List<MovePath.Move> moves, float time, float speed,
		out Vector2 position, out Vector2 direction, out MovePath.Move move) {

		var accelerationTime = 1 / speed;
		var acceleration = speed * speed;
		var rotation90Time = Mathf.PI / 4 / speed;

		float getTime(MovePath.MoveType type) {
			return type switch {
				MovePath.MoveType.Start or MovePath.MoveType.Stop => accelerationTime,
				MovePath.MoveType.RotateLeft or MovePath.MoveType.RotateRight => rotation90Time,
				MovePath.MoveType.RotateBack => rotation90Time * 2,
				MovePath.MoveType.Forward => 1 / speed,
				MovePath.MoveType.SteerLeft or MovePath.MoveType.SteerRight => (float)Mathf.PI / 4 / speed,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}

		queue.Clear();
		foreach (var m in moves)
			queue.Enqueue(m);

		if (time < 0)
			time = 0;
		while (queue.Count > 0 && getTime(queue.Peek().type) < time)
			time -= getTime(queue.Dequeue().type);

		var reachedEnd = false;
		if (queue.Count == 0) {
			move = moves.Last();
			time = accelerationTime;
			reachedEnd = true;
		}
		else
			move = queue.Dequeue();

		switch (move.type) {

			case MovePath.MoveType.Forward:
				position = move.midpoint + (Vector2)move.forward * time * speed;
				direction = move.forward;
				break;

			case MovePath.MoveType.SteerLeft:
			case MovePath.MoveType.SteerRight: {
				var t = time / getTime(move.type);
				var angle = t * Mathf.PI / 2;
				var archPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .5f;
				position = new Vector2(archPosition.x - .5f, archPosition.y);
				direction = new Vector2(-archPosition.y, archPosition.x).normalized;
				if (move.type == MovePath.MoveType.SteerRight) {
					position = Vector2.Scale(position, new Vector2(-1, 1));
					direction = Vector2.Scale(direction, new Vector2(-1, 1));
				}

				var rotation = Vector2Int.up.Rotation(move.forward);
				position = position.Rotate(rotation);
				direction = direction.Rotate(rotation);

				position += move.midpoint;
				break;
			}

			case MovePath.MoveType.Start:
				position = move.midpoint + (Vector2)move.forward * (acceleration * time * time / 2);
				direction = move.forward;
				break;

			case MovePath.MoveType.Stop:
				position = move.midpoint + (Vector2)move.forward * (speed * time - acceleration * time * time / 2);
				direction = move.forward;
				break;

			case MovePath.MoveType.RotateLeft:
			case MovePath.MoveType.RotateRight:
			case MovePath.MoveType.RotateBack: {

				position = move.midpoint;

				var rotation = move.type switch {
					MovePath.MoveType.RotateLeft => 1,
					MovePath.MoveType.RotateBack => 2,
					MovePath.MoveType.RotateRight => 3
				};
				var deltaAngle = Vector2.SignedAngle(move.forward, move.forward.Rotate(rotation));
				var t = time / (move.type == MovePath.MoveType.RotateBack ? 2 * rotation90Time : rotation90Time);
				direction = Quaternion.AngleAxis(deltaAngle * t, Vector3.forward) * (Vector2)move.forward;
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}

		return reachedEnd;
	}
}