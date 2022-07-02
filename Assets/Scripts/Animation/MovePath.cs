using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovePath  {
	
	public enum MoveType { Forward, Left, Right, Back }

	public IReadOnlyList<Vector2> Points => points;
	public IReadOnlyList<Vector2Int> Tiles => tiles;
	public IReadOnlyList<((Vector2 position, Vector2Int direction) start, (Vector2 position, Vector2Int direction) end, MoveType type)> Moves  =>moves;

	private List<Vector2> points = new();
	private List<Vector2Int> tiles = new();
	private List<((Vector2 position, Vector2Int direction) start, (Vector2 position, Vector2Int direction) end, MoveType type)> moves = new();
	
	public static float AccelerationTime(float speed) => 1 / speed;
	public static float Acceleration(float speed) => speed * speed;

	public bool IsEmpty => Tiles.Count <= 1;

	public float Duration(float speed, float rotation90Time) =>
		IsEmpty ? 0 : Moves.Sum(move => Time(move.type, speed, rotation90Time)) + 2 * AccelerationTime(speed);

	public static float Time(MoveType type, float speed, float rotation90Time) {
		return type switch {
			MoveType.Forward => 1 / speed,
			MoveType.Back => 2 * AccelerationTime(speed) + rotation90Time * 2,
			MoveType.Left or MoveType.Right => (float)Math.PI / 4 / speed,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public MovePath(IEnumerable<Vector2> points, bool visualize=false) {
		IEnumerable<TResult> pairs<T, TResult>(ICollection<T> sequence, Func<T, T, TResult> selector)
			=> sequence.Take(sequence.Count - 1).Zip(sequence.Skip(1), selector);

		this.points.Clear();
		tiles.Clear();
		moves.Clear();
		
		if (points == null) {
			return;
		}
		
		this.points.AddRange(points);
		if (Points.Count == 0)
			return;
		
		var startTile = new Vector2Int(Mathf.RoundToInt(Points[0].x), y: Mathf.RoundToInt(Points[0].y));
		var segments = pairs(this.points, (start, end) => (start, end));

		tiles.Add(startTile);
		tiles.AddRange(segments.SelectMany(segment => Woo.Traverse2D(segment.start, segment.end)));

		var midpoints = pairs(tiles, (a, b) => (position: Vector2.Lerp(a, b, .5f), direction: b - a)).ToList();
		moves.AddRange(pairs(midpoints, (start, end) => {
			MoveType type;
			if (start.direction == end.direction)
				type = MoveType.Forward;
			else if (start.direction == -end.direction)
				type = MoveType.Back;
			else
				type = start.direction.Cross(end.direction) == 1 ? MoveType.Left : MoveType.Right;
			return (start, end, type);
		}));
	}

	public bool Sample(float speed, float rotation90Time, float targetTime, out Vector2 position, out Vector2 direction) {
		switch (Tiles.Count) {
			case 0:
				position = direction = default;
				return false;
			case 1:
				position = Tiles[0];
				direction = default;
				return false;
		}

		var duration = Duration(speed, rotation90Time);
		var accelerationTime = AccelerationTime(speed);
		var acceleration = Acceleration(speed);
		var time = Math.Clamp(targetTime, 0, duration);

		// start
		if (time < accelerationTime) {
			var p = Tiles[0];
			var d = Tiles[1] - Tiles[0];
			position = p + (Vector2)d * acceleration * time * time / 2;
			direction = d;
			return true;
		}

		// stop
		if (time > duration - accelerationTime) {
			time -= duration - accelerationTime;
			var p = Tiles[^2];
			var d = Tiles[^1] - p;
			position = p + (Vector2)d * (.5f + speed * time - acceleration * time * time / 2);
			direction = d;
			return true;
		}

		// find move
		var move = Moves[^1];
		time -= accelerationTime;
		for (var i = 0; i < Moves.Count; i++) {
			var moveTime = Time(Moves[i].type, speed, rotation90Time);
			if (time < moveTime) {
				move = Moves[i];
				break;
			}
			time -= moveTime;
		}

		//
		switch (move.type) {
			case MoveType.Forward:
				position = move.start.position + (Vector2)move.start.direction * time * speed;
				direction = move.end.position - move.start.position;
				return true;

			case MoveType.Left:
			case MoveType.Right: {
				var t = time / Time(move.type, speed, rotation90Time);
				var angle = t * Mathf.PI / 2;
				var archPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .5f;
				position = new Vector2(archPosition.x - .5f, archPosition.y);
				direction = new Vector2(-archPosition.y, archPosition.x).normalized;
				if (move.type == MoveType.Right) {
					position = Vector2.Scale(position, new Vector2(-1, 1));
					direction = Vector2.Scale(direction, new Vector2(-1, 1));
				}

				var rotation = Vector2Int.up.Rotation(move.start.direction);
				position = position.Rotate(rotation);
				direction = direction.Rotate(rotation);

				position += move.start.position;
				return true;
			}

			case MoveType.Back:
				if (time < accelerationTime || time > accelerationTime + rotation90Time * 2) {
					if (time > accelerationTime + rotation90Time * 2)
						time -= rotation90Time * 2;

					position = move.start.position + (Vector2)move.start.direction * (speed * time - acceleration * time * time / 2);
					direction = time < accelerationTime ? move.start.direction : -move.start.direction;
					return true;
				}
				else {
					position = move.start.position + (Vector2)move.start.direction * .5f;
					var t = (time - accelerationTime) / (rotation90Time * 2);
					var angle = Mathf.SmoothStep(Mathf.PI / 2, Mathf.PI * 1.5f, t);
					direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

					var rotation = Vector2Int.up.Rotation(move.start.direction);
					direction = direction.Rotate(rotation);
					return true;
				}

			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}