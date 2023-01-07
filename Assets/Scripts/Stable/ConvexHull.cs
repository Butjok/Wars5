/* This is taken from this blog post:
 * http://loyc-etc.blogspot.ca/2014/05/2d-convex-hull-in-c-45-lines-of-code.html
 *
 * All I have done is renamed "DList" to "CircularList" and then wrote a wrapper for the generic C# list.
 * The structure that is supposed to be used is *much* more efficient, but this works for my purposes.
 *
 * This can be dropped right into your Unity project and will work without any adjustments.
 */

using System.Collections.Generic;
using UnityEngine;

public static class ConvexHull {
	public static CircularList<Vector2> Compute(List<Vector2> points, bool sortInPlace = false){
		if(!sortInPlace)
			points = new List<Vector2>(points);

		points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x > b.x ? 1 : -1);

		// Importantly, DList provides O(1) insertion at beginning and end
		var hull = new CircularList<Vector2>();
		int L = 0, U = 0; // size of lower and upper hulls

		// Builds a hull such that the output polygon starts at the leftmost Vector2.
		for(var i = points.Count - 1; i >= 0; i--){
			Vector2 p = points[i], p1;

			// build lower hull (at end of output list)
			while (L >= 2 && ((p1 = hull.Last) - hull[hull.Count - 2]).Cross(p - p1) >= 0){
				hull.PopLast();
				L--;
			}
			hull.PushLast(p);
			L++;

			// build upper hull (at beginning of output list)
			while (U >= 2 && ((p1 = hull.First) - hull[1]).Cross(p - p1) <= 0){
				hull.PopFirst();
				U--;
			}
			if(U != 0) // when U=0, share the Vector2 added above
				hull.PushFirst(p);
			U++;
			Debug.Assert(U + L == hull.Count + 1);
		}
		hull.PopLast();
		return hull;
	}
	
	public static Vector2 ClosestPoint(List<Vector2>points, Vector2 point) {

		var minDistance = float.MaxValue;
		Vector2 result = default;

		for(var i = 0; i < points.Count; i++) {
			var start = points[i];
			var end = points[(i + 1) % points.Count];
			var edge = end - start;
			var length = edge.magnitude;
			var direction = edge / length;
			var vector = point - start;
			var side = direction.Cross(vector);
			if(side > 0) {
				var projectedLength = Vector2.Dot(vector, direction);
				Vector2 closest;
				if(projectedLength >= 0 && projectedLength <= length)
					closest = start + direction * projectedLength;
				else if(projectedLength < 0)
					closest = start;
				else
					continue;
				var distance = Vector2.Distance(point, closest);
				if(distance < minDistance) {
					minDistance = distance;
					result = closest;
				}
			}
		}

		return minDistance == float.MaxValue ? point : result;
	}

	public class CircularList<T> : List<T> {
		public T Last => this[Count - 1];
		public T First => this[0];
		public void PushLast(T obj){
			Add(obj);
		}
		public void PopLast(){
			RemoveAt(Count - 1);
		}
		public void PushFirst(T obj){
			Insert(0, obj);
		}
		public T PopFirst(){
			var retVal = this[0];
			RemoveAt(0);
			return retVal;
		}
	}
}