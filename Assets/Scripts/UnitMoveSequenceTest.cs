using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;

public class UnitMoveSequenceTest : MonoBehaviour {
    public List<Vector2Int> positions = new();
    public float speed = 3;
    public bool rotateAtTheEnd = true;
    public Vector2Int finalDirection = Vector2Int.up;

    [Command]
    public void Move() {
        StopAllCoroutines();
        // StartCoroutine(Create(transform, positions, speed, rotateAtTheEnd ? finalDirection : null));
    }
}