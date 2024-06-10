using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class Crate : IMaterialized {
    public static readonly List<Crate> toDispose = new();

    public Level level;
    public Vector2Int position;

    [DontSave] public bool IsMaterialized { get; private set; }
    [DontSave] public CrateView view;

    public void Materialize() {
        Assert.IsFalse(IsMaterialized);
        Assert.IsFalse(toDispose.Contains(this));
        toDispose.Add(this);

        var prefab = "Crate".LoadAs<CrateView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;

        IsMaterialized = true;
    }

    public void Dematerialize() {
        Assert.IsTrue(toDispose.Contains(this));
        toDispose.Remove(this);

        if (view) {
            Object.Destroy(view.gameObject);
            view = null;
        }

        IsMaterialized = false;
    }
    
    public void PickUp(Unit unit) {
        Assert.IsTrue(level.crates.TryGetValue(position, out var crate) && crate == this);
        level.crates.Remove(position);
        Dematerialize();
        
        Debug.Log("Crate was picked up.");
        unit.Player.SetCredits(unit.Player.Credits + 10000, true);
    }

    [Command]
    public static void Place(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null) {
            if (level.crates.TryGetValue(position, out var oldCrate)) {
                oldCrate.Dematerialize();
                level.crates.Remove(position);
            }
            var crate = new Crate {
                level = level,
                position = position
            };
            level.crates[position] = crate;
            crate.Materialize();
        }
    }
    [Command]
    public static void Remove(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null && level.crates.TryGetValue(position, out var crate)) {
            crate.Dematerialize();
            level.crates.Remove(position);
        }
    }
}