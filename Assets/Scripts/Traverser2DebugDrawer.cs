using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
// ReSharper disable LoopCanBePartlyConvertedToQuery

public class Traverser2DebugDrawer : MonoBehaviour {


}

public class Traverser2 : Dictionary<Vector2Int, Traverser2.Tile> {

	public struct Tile {
		public int cost;
		public int distance;
		public PreviousPositions cameFrom;
	}

	public void Add(Vector2Int position, int cost) {
		Assert.IsFalse(ContainsKey(position), $"{position} is already set");
		base.Add(position, new Tile { cost = cost });
	}
	public bool IsReachable(Vector2Int position) {
		return TryGetValue(position, out var tile) && tile.distance < infinity;
	}
	public IEnumerable<Vector2Int> ReachablePositions => Keys.Where(IsReachable);

	private readonly SimplePriorityQueue<Vector2Int> priorityQueue = new();
	public const int infinity = 9999;
	public static readonly IReadOnlyList<Vector2Int> offsets = new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

	public void Traverse(Vector2Int targetPosition, Vector2Int unitPosition) {

		int DistanceToUnit(Vector2Int position) => useHeuristic ? (position - unitPosition).ManhattanLength() : 0;

		priorityQueue.Clear();
		foreach (var position in Keys.ToList()) {
			var tile = this[position];
			tile.distance = position == targetPosition ? 0 : infinity;
			this[position] = tile;
			priorityQueue.Enqueue(position, tile.distance + DistanceToUnit(position));
		}

		while (priorityQueue.TryDequeue(out var currentPosition)) {
			var current = this[currentPosition];
			if (currentPosition == unitPosition || current.distance >= infinity)
				break;

			foreach (var offset in offsets) {
				var neighborPosition = currentPosition + offset;

				if (priorityQueue.Contains(neighborPosition) && TryGetValue(neighborPosition, out var neighbor)) {

					var alternativeDistance = current.distance + neighbor.cost;
					if (alternativeDistance < neighbor.distance) {
						neighbor.distance = alternativeDistance;
						neighbor.cameFrom.Clear();
						neighbor.cameFrom.Add(currentPosition);
						priorityQueue.UpdatePriority(neighborPosition, neighbor.distance + DistanceToUnit(neighborPosition));
					}
					else if (alternativeDistance == neighbor.distance)
						neighbor.cameFrom.Add(currentPosition);

					this[neighborPosition] = neighbor;
				}
			}
		}
	}


	[Command] public static Vector2Int goal = new Vector2Int(-6, -17);
	public static Traverser2DebugDrawer drawer;

	[Command]
	public static bool SetGoal() {
		if (!Mouse.TryGetPosition(out Vector2Int mousePosition))
			return false;
		goal = mousePosition;
		return true;
	}


	[Command]
	public static void Test2() {
		if (!drawer) {
			var go = new GameObject(nameof(Traverser2DebugDrawer));
			drawer = go.AddComponent<Traverser2DebugDrawer>();
		}
		drawer.StopAllCoroutines();
		drawer.StartCoroutine(Test2Enumerator());
	}

	[Command] public static float thickness = 2;
	[Command] public static Color arrowColor = Color.yellow;
	[Command] public static Color pathColor = Color.blue;
	[Command] public static Color tileColor = Color.yellow * new Color(1, 1, 1, .2f);
	[Command] public static Color textColor = Color.black;
	[Command] public static float textSize = 14;
	[Command] public static LabelAlignment textAlignment = LabelAlignment.Center;
	[Command] public static float arrowSize = .1f;
	[Command] public static Vector2 arrowLerp = new Vector2(.15f, .85f);

	public IEnumerable<IEnumerable<Vector2Int>> GetAllSubpathsFrom(Vector2Int position) {
		if (!TryGetValue(position, out var tile))
			yield break;

		if (tile.cameFrom.Count == 0)
			yield return new[] { position };

		foreach (var nextPosition in tile.cameFrom)
		foreach (var path in GetAllSubpathsFrom(nextPosition))
			yield return path.Prepend(position);
	}

	public List<Vector2Int> TruncatePathToStay(IEnumerable<Vector2Int> fullPath, Unit unit) {
		var moveDistance = Rules.MoveCapacity(unit);
		var validPath = new List<Vector2Int>();
		var isFirst = true;
		var distanceAtUnit = 0;
		foreach (var position in fullPath) {
			if (isFirst) {
				isFirst = false;
				distanceAtUnit = this[position].distance;
			}
			else {
				var distance = distanceAtUnit - this[position].distance;
				if (distance > moveDistance || !Rules.CanStay(unit, position))
					break;
			}
			validPath.Add(position);
		}
		return validPath;
	}

	[Command] public static int alternativePathsLimit = 10;
	[Command] public static bool useHeuristic = true;

	public List<Vector2Int> FindPathToStay(Unit unit, Vector2Int goal) {

		if (unit.Position is not { } unitPosition)
			throw new AssertionException("unit.Position == null", null);

		Clear();
		foreach (var position in unit.Player.level.tiles.Keys) {
			if (!Rules.TryGetMoveCost(unit, unit.Player.level.tiles[position], out var moveCost))
				continue;
			Add(position, moveCost);
		}
		Traverse(goal, unitPosition);

		List<Vector2Int> bestPath = null;
		var minDistance = int.MaxValue;
		var pathNumber = 0;
		foreach (var path in GetAllSubpathsFrom(unitPosition)) {
			var pathToStay =  TruncatePathToStay(path, unit);
			var distance = this[pathToStay[^1]].distance;
			if (distance < minDistance) {
				minDistance = distance;
				bestPath = pathToStay;
			}
			pathNumber++;
			if (pathNumber > alternativePathsLimit)
				break;
		}
		Assert.IsNotNull(bestPath);

		return bestPath;
	}

	public static IEnumerator Test2Enumerator() {
		var main = Object.FindObjectOfType<Level>();
		Assert.IsTrue(main);

		if (!Mouse.TryGetPosition(out Vector2Int mousePosition) || !main.TryGetUnit(mousePosition, out var unit))
			yield break;


		var traverser = new Traverser2();
		foreach (var position in unit.Player.level.tiles.Keys) {
			if (!Rules.TryGetMoveCost(unit, unit.Player.level.tiles[position], out var moveCost))
				continue;
			traverser.Add(position, moveCost);
		}
		traverser.Traverse(goal, mousePosition);

		foreach (var path in traverser.GetAllSubpathsFrom(mousePosition)) {

			var pathToStay = path.ToList();// traverser.TruncatePathToStay(path, unit);

			while (!Input.GetKeyDown(KeyCode.Alpha0)) {
				yield return null;

				// draw tiles + came from
				using (Draw.ingame.WithLineWidth(thickness)) {

					foreach (var position in traverser.ReachablePositions) {
						var tile = traverser[position];
						Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, tileColor);
						Draw.ingame.Label2D((Vector3)position.ToVector3Int(), tile.distance.ToString(), textSize, textAlignment, textColor);
						foreach (var cameFrom in tile.cameFrom) {
							var from = (Vector3)cameFrom.ToVector3Int();
							var to = (Vector3)position.ToVector3Int();
							Draw.ingame.Arrow(Vector3.Lerp(from, to, arrowLerp[0]), Vector3.Lerp(from, to, arrowLerp[1]), Vector3.up, arrowSize, arrowColor);
						}
					}

					for (var i = 1; i < pathToStay.Count; i++) {
						var from = (Vector3)pathToStay[i].ToVector3Int();
						var to = (Vector3)pathToStay[i - 1].ToVector3Int();
						Draw.ingame.Arrow(Vector3.Lerp(from, to, arrowLerp[0]), Vector3.Lerp(from, to, arrowLerp[1]), Vector3.up, arrowSize, pathColor);
					}
				}
			}
			yield return null;
		}
	}
}