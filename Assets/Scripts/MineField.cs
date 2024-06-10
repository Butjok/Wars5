using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class MineField : ISpawnable {
    public static readonly List<MineField> spawned = new();

    public Level level;
    public Vector2Int position;
    private Player player;

    [DontSave] public bool IsSpawned { get; private set; }
    [DontSave] public MineFieldView view;

    [DontSave] public Player Player {
        get => player;
        set {
            player = value;
            if (IsSpawned) 
                view.PlayerColor = player?.Color ?? Color.clear;
        }
    }

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        var prefab = "MineField".LoadAs<MineFieldView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;
        view.PlayerColor = player?.Color ?? Color.clear;

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
    public static void Place(string playerColor, Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null) {
            var player = level.players.Find(player => player.ColorName.ToString() == playerColor);
            if (level.mineFields.TryGetValue(position, out var oldMineField)) {
                oldMineField.Despawn();
                level.mineFields.Remove(position);
            }
            var mineField = new MineField {
                level = level,
                position = position,
                player = player
            };
            level.mineFields[position] = mineField;
            mineField.Spawn();
        }
    }
    [Command]
    public static void Remove(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null && level.mineFields.TryGetValue(position, out var mineField)) {
            mineField.Despawn();
            level.mineFields.Remove(position);
        }
    }
}