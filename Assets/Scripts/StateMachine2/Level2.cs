using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level2 : StateMachine2<Level2> {
    
    public Dictionary<Vector2Int, TileType> tiles = new();
    
    public void Awake() {
        tiles[new Vector2Int(1, 1)] = TileType.Plain;
        tiles[new Vector2Int(2, 1)] = TileType.Road;
        tiles[new Vector2Int(3, 1)] = TileType.Plant;

        var state = new SelectionState2(this); 
        RunWith(state);
        RunWith(state);
    }
}

public class SelectionState2 : State2<Level2> {
    public int index = -1;
    public Vector2Int? position;
    public List<Vector2Int> buildings;
    public SelectionState2(Level2 parent) : base(parent) {
        buildings = parent.tiles.Keys.Where(position => parent.tiles[position] == TileType.Plant).ToList();
    }
    public override void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (buildings.Count > 0) {
                index = (index + 1) % buildings.Count;
                position = buildings[index];
                Debug.Log(position);
            }
            else
                Debug.Log("not allowed");
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (position is { } value) 
                ChangeTo(new PathSelectionState2(parent, value));
            else
                Debug.Log("not allowed");
        }
    }
}

public class PathSelectionState2 : State2<Level2> {
    public Vector2Int position;
    public PathSelectionState2(Level2 parent, Vector2Int position) : base(parent) {
        this.position = position;
    }
    public override void Start() {
        Debug.Log(position);
    }
    public override void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ChangeTo(new SelectionState2(parent));
        }
    }
}