using System;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public class Traverser {

	private struct Info {
		public int distance;
		public Vector2Int? previous;
	}

	private Dictionary<Vector2Int, Info> infos = new();
	private SimplePriorityQueue<Vector2Int> queue = new();

	public void Traverse(IEnumerable<Vector2Int> positions, Vector2Int start, Func<Vector2Int, int, int?> cost, int maxDistance) {

		infos.Clear();
		queue.Clear();

		foreach (var position in positions) {
			var distance = position == start ? 0 : int.MaxValue;
			infos[position] = new Info { distance = distance, previous = null };
			queue.Enqueue(position, distance);
		}

		while (queue.Count > 0) {

			var position = queue.Dequeue();
			var distance = infos[position].distance;
			if (distance > maxDistance)
				break;

			foreach (var offset in Rules.offsets) {

				var neighbor = position + offset;
				if (!infos.TryGetValue(neighbor, out var neighborInfo) ||
				    !queue.Contains(neighbor) ||
				    cost(neighbor, distance) is not { } cost2)
					continue;

				var alternativeDistance = distance + cost2;
				if (alternativeDistance < neighborInfo.distance) {
					infos[neighbor] = new Info {
						distance = alternativeDistance,
						previous = position
					};
					queue.UpdatePriority(neighbor, alternativeDistance);
				}
			}
		}
	}

	public List<Vector2Int> ReconstructPath(Vector2Int target) {

		if (!infos.TryGetValue(target, out var info) || info.distance == int.MaxValue)
			return null;

		var result = new List<Vector2Int>();
		for (Vector2Int? position = target; position != null; position = infos[(Vector2Int)position].previous)
			result.Add((Vector2Int)position);
		result.Reverse();
		return result;
	}

	public int GetDistance(Vector2Int position) {
		return infos.TryGetValue(position, out var info) ? info.distance : int.MaxValue;
	}
	public bool IsReachable(Vector2Int position, int maxDistance) {
		return GetDistance(position) <= maxDistance;
	}
}