using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovePath {

	public enum MoveType {
		Start, Stop, Forward, SteerLeft, SteerRight, RotateBack, RotateLeft, RotateRight
	}

	[Serializable]
	public struct Move {
		public MoveType type;
		public Vector2 start;
		public Vector2Int forward;
		public Move(MoveType type, Vector2 start, Vector2Int forward) {
			this.type = type;
			this.start = start;
			this.forward = forward;
		}
		public Move(MoveType type, (Vector2 position, Vector2Int forward) midpoint)
			: this(type, midpoint.position, midpoint.forward) { }
	}

	public static List<Move> From(IList<Vector2> points, Vector2Int startPosition, Vector2Int startForward) {

		if (points is not { Count: > 1 })
			return null;

		var tiles = new List<Vector2Int> { points[0].RoundToInt() };
		for (var i = 1; i < points.Count; i++)
			tiles.AddRange(Woo.Traverse2D(points[i - 1], points[i]));

		var midpoints = new List<(Vector2 position, Vector2Int forward)>();
		for (var i = 1; i < tiles.Count; i++) {
			var previous = tiles[i - 1];
			var current = tiles[i];
			midpoints.Add((Vector2.Lerp(previous, current, .5f), current - previous));
		}

		var moves = new List<Move>();

		if (startForward == -midpoints[0].forward)
			moves.Add(new Move(MoveType.RotateBack, startPosition, startForward));
		else if (startForward != midpoints[0].forward)
			moves.Add(new Move(startForward.Cross(midpoints[0].forward) == 1 ? MoveType.RotateLeft : MoveType.RotateRight, startPosition, startForward));

		moves.Add(new Move(MoveType.Start, startPosition, midpoints[0].forward));

		for (var i = 1; i < midpoints.Count; i++) {
			var start = midpoints[i - 1];
			var end = midpoints[i];
			if (start.forward == end.forward)
				moves.Add(new Move(MoveType.Forward, start));
			else if (start.forward == -end.forward) {
				moves.Add(new Move(MoveType.Stop, start));
				moves.Add(new Move(MoveType.RotateBack, start.position + (Vector2)start.forward / 2, start.forward));
				moves.Add(new Move(MoveType.Start, start.position + (Vector2)start.forward / 2, -start.forward));
			}
			else
				moves.Add(new Move(start.forward.Cross(end.forward) == 1 ? MoveType.SteerLeft : MoveType.SteerRight, start));
		}
		moves.Add(new Move(MoveType.Stop, midpoints.Last()));

		return moves;
	}
	
	public static Queue<Move> queue = new();

	public static bool Sample(List<Move> moves, float time, float speed,
		out Vector2 position, out Vector2 direction, out Move move) {
		
		var accelerationTime = 1 / speed;
		var acceleration = speed * speed;
		var rotation90Time = Mathf.PI / 4 / speed;
		
		float getTime(MoveType type) {
			return type switch {
				MoveType.Start or MoveType.Stop => accelerationTime,
				MoveType.RotateLeft or MoveType.RotateRight => rotation90Time,
				MoveType.RotateBack => rotation90Time * 2,
				MoveType.Forward => 1 / speed,
				MoveType.SteerLeft or MoveType.SteerRight => (float)Mathf.PI / 4 / speed,
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

		var clamped = false;
		if (queue.Count == 0) {
			move = moves.Last();
			time = accelerationTime;
			clamped = true;
		}
		else
			move = queue.Dequeue();
		
		switch (move.type) {

			case MoveType.Forward:
				position = move.start + (Vector2)move.forward * time * speed;
				direction = move.forward;
				break;

			case MoveType.SteerLeft:
			case MoveType.SteerRight: {
				var t = time / getTime(move.type);
				var angle = t * Mathf.PI / 2;
				var archPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .5f;
				position = new Vector2(archPosition.x - .5f, archPosition.y);
				direction = new Vector2(-archPosition.y, archPosition.x).normalized;
				if (move.type == MoveType.SteerRight) {
					position = Vector2.Scale(position, new Vector2(-1, 1));
					direction = Vector2.Scale(direction, new Vector2(-1, 1));
				}

				var rotation = Vector2Int.up.Rotation(move.forward);
				position = position.Rotate(rotation);
				direction = direction.Rotate(rotation);

				position += move.start;
				break;
			}

			case MoveType.Start:
				position = move.start + (Vector2)move.forward * (acceleration * time * time / 2);
				direction = move.forward;
				break;

			case MoveType.Stop:
				position = move.start + (Vector2)move.forward * (speed * time - acceleration * time * time / 2);
				direction = move.forward;
				break;

			case MoveType.RotateLeft:
			case MoveType.RotateRight:
			case MoveType.RotateBack: {

				position = move.start;

				var rotation = move.type switch {
					MoveType.RotateLeft => 1,
					MoveType.RotateBack => 2,
					MoveType.RotateRight => 3
				};
				var deltaAngle = Vector2.SignedAngle(move.forward, move.forward.Rotate(rotation));
				var t = time / (move.type == MoveType.RotateBack ? 2 * rotation90Time : rotation90Time);
				direction = Quaternion.AngleAxis(deltaAngle * t, Vector3.forward) * (Vector2)move.forward;
				break;
			}

			default:
				throw new ArgumentOutOfRangeException();
		}

		return clamped;
	}
}