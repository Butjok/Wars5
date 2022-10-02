using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MovePath {

	public enum MoveType {
		Start, Stop, Forward, SteerLeft, SteerRight, RotateBack, RotateLeft, RotateRight
	}

	[Serializable]
	public struct Move {
		
		public MoveType type;
		public Vector2 midpoint;
		public Vector2Int forward;
		
		public Move(MoveType type, Vector2 midpoint, Vector2Int forward) {
			this.type = type;
			this.midpoint = midpoint;
			this.forward = forward;
		}
		public Move(MoveType type, (Vector2 position, Vector2Int forward) midpoint)
			: this(type, midpoint.position, midpoint.forward) { }
	}

	public List<Vector2Int> positions;
	public List<Move> moves;

	public MovePath(IReadOnlyList<Vector2> points, Vector2Int startForward) {
		positions = Positions(points).ToList();
		moves = Moves(positions, startForward);
	}
	
	public MovePath(IReadOnlyList<Vector2Int> positions, Vector2Int startForward) {
		this.positions = positions.ToList();
		moves = Moves(positions, startForward);
	}

	public static IEnumerable<Vector2Int> Positions(IReadOnlyList<Vector2> points) {
		var positions = new List<Vector2Int> { points[0].RoundToInt() };
		for (var i = 1; i < points.Count; i++)
			positions.AddRange(Woo.Traverse2D(points[i - 1], points[i]));
		return positions;
	}

	public static List<Move> Moves(IReadOnlyList<Vector2Int> positions, Vector2Int startForward) {
		
		var moves = new List<Move>();

		if (positions.Count <= 1)
			return moves;
		
		var midpoints = new List<(Vector2 position, Vector2Int forward)>();
		for (var i = 1; i < positions.Count; i++) {
			var previous = positions[i - 1];
			var current = positions[i];
			midpoints.Add((Vector2.Lerp(previous, current, .5f), current - previous));
		}

		if (startForward == -midpoints[0].forward)
			moves.Add(new Move(MoveType.RotateBack, positions[0], startForward));
		else if (startForward != midpoints[0].forward)
			moves.Add(new Move(startForward.Cross(midpoints[0].forward) == 1 ? MoveType.RotateLeft : MoveType.RotateRight, positions[0], startForward));

		moves.Add(new Move(MoveType.Start, positions[0], midpoints[0].forward));

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
}