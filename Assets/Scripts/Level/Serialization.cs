using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Vector2IntUtils {
    public static Vector2Int ToVector2Int(this (int x, int y) tuple) {
        return new Vector2Int(tuple.x, tuple.y);
    }
    public static (int x, int y) ToTuple(this Vector2Int vector) {
        return (vector.x, vector.y);
    }
}

public class Numerator {
    public Dictionary<object, int> ids = new();
    public int this[object reference] {
        get {
            if (reference == null)
                return -1;
            if (!ids.TryGetValue(reference, out var id))
                id = ids[reference] = ids.Count;
            return id;
        }
    }
}

public class SerializedPlayer {

    public int id;
    public Team team;
    public Color32 color;
    public string coName;
    public PlayerType type;
    public AiDifficulty difficulty;
    public int credits;

    public SerializedPlayer() { }
    public SerializedPlayer(Player player, Numerator id) {
        this.id = id[player];
        team = player.team;
        color = player.color;
        if (player.co)
            coName = player.co.name;
        type = player.type;
        difficulty = player.difficulty;
        credits = player.credits;
    }

    public override string ToString() {
        return color.Name();
    }
}

public class SerializedUnit {

    public int id;
    public UnitType type;
    public int playerId;
    public (int x, int y)? position;
    public bool moved;
    public int hp=10;
    public int fuel=999;
    public int[] ammo={99,99,99};
    public int[] cargo={};

    public SerializedUnit() { }
    public SerializedUnit(Unit unit, Numerator id) {
        this.id = id[unit];
        type = unit.type;
        playerId = id[unit.player];
        position = unit.position.v?.ToTuple();
        moved = unit.moved.v;
        hp = unit.hp.v;
        fuel = unit.fuel.v;
        ammo = unit.ammo?.ToArray();
        cargo = unit.cargo?.Select(u => id[u]).ToArray();
    }

    public override string ToString() {
        return $"{type}{position} {playerId}";
    }

    public bool HasSameData(SerializedUnit other) {
        return type == other.type &&
               playerId == other.playerId &&
               Nullable.Equals(position, other.position) &&
               moved == other.moved &&
               hp == other.hp &&
               fuel == other.fuel &&
               ammo.SequenceEqual(other.ammo) &&
               cargo.SequenceEqual(other.cargo);
    }
}

public class SerializedBuilding {

    public int id;
    public TileType type;
    public (int x, int y) position;
    public int playerId;
    public int cp=20;

    public SerializedBuilding() { }
    public SerializedBuilding(Building building, Numerator id) {
        this.id = id[building];
        type = building.type;
        position = building.position.ToTuple();
        playerId = id[building.player.v];
        cp = building.cp.v;
    }

    public override string ToString() {
        return $"{type}{position} {playerId}";
    }

    public bool HasSameData(SerializedBuilding other) {
        return type == other.type && 
               position.Equals(other.position) && 
               playerId == other.playerId && 
               cp == other.cp;
    }
}

public class LevelConfiguration {

    public SerializedUnit[] units={};
    public ((int x, int y) position, TileType tileType)[] tiles={};
    public SerializedPlayer[] players={};
    public SerializedBuilding[] buildings={};

    // public SerializedGame() { }
    // public SerializedGame(Game game) {
    //
    //     var units = game.units.Values.ToList();
    //     void addCargo(Unit unit) {
    //         if (unit.cargo == null)
    //             return;
    //         foreach (var cargo in unit.cargo.Where(cargo => !units.Contains(cargo))) {
    //             units.Add(cargo);
    //             addCargo(cargo);
    //         }
    //     }
    //     foreach (var unit in game.units.Values)
    //         addCargo(unit);
    //
    //     var id = new Numerator();
    //     this.units = units.Select(unit => new SerializedUnit(unit, id)).ToArray();
    //     tiles = game.tiles.Select(kv => new SerializedTile(kv.Key, kv.Value)).ToArray();
    //     players = game.players.Select(player => new SerializedPlayer(player, id)).ToArray();
    //     turn = game.turn;
    //     buildings = game.buildings.Values.Select(building => new SerializedBuilding(building, id)).ToArray();
    // }
}