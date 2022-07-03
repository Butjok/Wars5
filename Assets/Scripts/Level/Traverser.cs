using System;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

public class Traverser {

	public struct Info {
		public int distance;
		public Vector2Int? previous;
	}

	public static Vector2Int[] offsets = { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };

	public Dictionary<Vector2Int, Info> infos = new();
	public SimplePriorityQueue<Vector2Int> queue;

	public void Traverse(IEnumerable<Vector2Int> positions, Vector2Int start, Func<Vector2Int, int, int?> cost) {

		infos.Clear();
		queue.Clear();

		foreach (var position in positions) {
			infos[position] = new Info { distance = int.MaxValue, previous = null };
			queue.Enqueue(position, int.MaxValue);
		}
		queue.UpdatePriority(start, 0);

		while (queue.Count > 0) {

			var position = queue.Dequeue();
			var distance = infos[position].distance;
			if (distance == int.MaxValue)
				break;

			foreach (var offset in offsets) {

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
	
	// todo: path reconstruction
}