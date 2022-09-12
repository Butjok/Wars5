using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Game2 : StateMachine2<Game2> {

    public Map2D<Unit> units;
    public  Map2D<TileType> tiles;
    public  Map2D<Building> buildings;
    public  List<Player> players = new();
    private int? turn;
    public LevelLogic levelLogic;

    public int? Turn {
        get => turn;
        set {
            turn = value;
            if (turn is { } integer && players.Count > 0) {
                var currentPlayer = players[integer.PositiveModulo(players.Count)];
                foreach (var player in players)
                    player.view.Visible = player == currentPlayer;
            }
        }
    }
    public Player CurrentPlayer {
        get {
            Assert.AreNotEqual(0, players.Count);
            if (turn is { } value)
                return players[value % players.Count];
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
}

public abstract class LevelLogic {
    public Game2 game;
    protected LevelLogic(Game2 game) {
        Assert.IsTrue(game);
        this.game = game;
    }
    public virtual void OnTurnStart(Game2 level) { }
    public virtual void OnTurnEnd(Game2 level) { }
    public virtual void OnActionCompletion(Game2 level) { }
}