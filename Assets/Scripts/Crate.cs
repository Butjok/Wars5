using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class Crate : ISpawnable {
    public static readonly List<Crate> spawned = new();

    public Level level;
    public Vector2Int position;

    [DontSave] public bool IsSpawned { get; private set; }
    [DontSave] public CrateView view;

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        var prefab = "Crate".LoadAs<CrateView>();
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
    
    public void PickUp(Unit unit) {
        Assert.IsTrue(level.crates.TryGetValue(position, out var crate) && crate == this);
        level.crates.Remove(position);
        Despawn();
        
        Debug.Log("Crate was picked up.");
        unit.Player.SetCredits(unit.Player.Credits + 10000, true);
    }

    [Command]
    public static void Place(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null) {
            if (level.crates.TryGetValue(position, out var oldCrate)) {
                oldCrate.Despawn();
                level.crates.Remove(position);
            }
            var crate = new Crate {
                level = level,
                position = position
            };
            level.crates[position] = crate;
            crate.Spawn();
        }
    }
    [Command]
    public static void Remove(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null && level.crates.TryGetValue(position, out var crate)) {
            crate.Despawn();
            level.crates.Remove(position);
        }
    }
}