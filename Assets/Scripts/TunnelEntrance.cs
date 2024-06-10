using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class TunnelEntrance : IMaterialized {

    public static readonly List<TunnelEntrance> toDematerialize = new();

    public Level level;
    public Vector2Int position;
    public TunnelEntrance connected;

    [DontSave] public TunnelEntranceView view;
    [DontSave] public bool IsMaterialized { get; private set; }
    
    public void Materialize() {
        Assert.IsFalse(IsMaterialized);
        Assert.IsFalse(toDematerialize.Contains(this));
        toDematerialize.Add(this);

        var prefab = "TunnelEntrance".LoadAs<TunnelEntranceView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;

        IsMaterialized = true;
    }
    public void Dematerialize() {
        Assert.IsTrue(toDematerialize.Contains(this));
        toDematerialize.Remove(this);

        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsMaterialized = false;
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