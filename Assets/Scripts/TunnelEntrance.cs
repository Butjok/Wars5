using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class TunnelEntrance : ISpawnable {

    public static readonly List<TunnelEntrance> spawned = new();

    public Level level;
    public Vector2Int position;
    public TunnelEntrance connected;

    [DontSave] public TunnelEntranceView view;
    [DontSave] public bool IsSpawned { get; private set; }
    
    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        var prefab = "TunnelEntrance".LoadAs<TunnelEntranceView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;

        IsSpawned = true;
    }
    public void Despawn() {
        Assert.IsTrue(spawned.Contains(this));
        spawned.Remove(this);

        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsSpawned = false;
    }

    [Command]
    public static bool TryConnect(Vector2Int positionA, Vector2Int positionB) {
        if (positionA == positionB)
            return false;
        var level = Game.Instance.TryGetLevel;
        if (level == null)
            return false;
        if (!level.TryGetTunnelEntrance(positionA, out var tunnelEntranceA) || !level.TryGetTunnelEntrance(positionB, out var tunnelEntranceB)) 
            return false;
        tunnelEntranceA.connected = tunnelEntranceB;
        tunnelEntranceB.connected = tunnelEntranceA;
        return true;
    }
}