using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Waypoint List", fileName = "WaypointList")]
public class WaypointList : ScriptableObject {

    public enum Type {
        Camera,
        Unit
    }

    public Type type;
    public List<Vector2Int> waypoints = new();
}