using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PathBuilder {

    public readonly Vector2Int startPosition;
    public List<Vector2Int> positions = new();
    public HashSet<Vector2Int> set = new();

    public PathBuilder(Vector2Int startPosition) {
        this.startPosition = startPosition;
        Clear();
    }

    public void Clear() {
        positions.Clear();
        positions.Add(startPosition);
        set.Clear();
        set.Add(startPosition);
    }

    public void Add(Vector2Int position) {

        if (!set.Contains(position)) {

            var previous = positions.Last();
            Assert.AreEqual(1, (position - previous).ManhattanLength());

            positions.Add(position);
            set.Add(position);
        }
        else
            for (var i = positions.Count - 1; i >= 0; i--) {
                if (positions[i] == position)
                    break;
                set.Remove(positions[i]);
                positions.RemoveAt(i);
            }
    }
}