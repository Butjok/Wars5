using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Game2 : StateMachine2<Game2> {

    public Map2D<Unit> units;
    public Map2D<TileType> tiles;
    public Map2D<Building> buildings;
    public List<Player> players = new();
    private int? turn = 0;
    public LevelLogic levelLogic;
    public Player realPlayer;

    public int? Turn {
        get => turn;
        set {
            turn = value;
            if (turn == null)
                return;
            foreach (var player in players)
                player.view.Visible = player == CurrentPlayer;
        }
    }
    public Player CurrentPlayer {
        get {
            Assert.AreNotEqual(0, players.Count);
            if (turn is { } integer)
                return players[integer % players.Count];
            throw new Exception();
        }
    }

    public bool TryGetTile(Vector2Int position, out TileType tile) {
        return tiles.TryGetValue(position, out tile) && tile != 0;
    }
    public bool TryGetUnit(Vector2Int position, out Unit unit) {
        return units.TryGetValue(position, out unit) && unit != null;
    }
    public bool TryGetBuilding(Vector2Int position, out Building building) {
        return buildings.TryGetValue(position, out building) && building != null;
    }

    public IEnumerable<Unit> FindUnitsOf(Player player) {
        return units.Values.Where(unit => unit.player == player);
    }
    public IEnumerable<Building> FindBuildingsOf(Player player) {
        return buildings.Values.Where(building => building.player.v == player);
    }

    public IEnumerable<Vector2Int> AttackPositions(Vector2Int position, Vector2Int range) {
        return range.Offsets().Select(offset => offset + position).Where(p => tiles.ContainsKey(p));
    }

    private void OnGUI() {
        GUILayout.Label($"Turn #{Turn} - {state.GetType().Name}");
    }
}