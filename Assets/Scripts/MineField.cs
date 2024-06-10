using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class MineField : IMaterialized {
    public static readonly List<MineField> toDematerialize = new();

    public Level level;
    public Vector2Int position;
    private Player player;

    [DontSave] public bool IsMaterialized { get; private set; }
    [DontSave] public MineFieldView view;

    [DontSave] public Player Player {
        get => player;
        set {
            player = value;
            if (IsMaterialized) 
                view.PlayerColor = player?.Color ?? Color.clear;
        }
    }

    public void Materialize() {
        Assert.IsFalse(IsMaterialized);
        Assert.IsFalse(toDematerialize.Contains(this));
        toDematerialize.Add(this);

        var prefab = "MineField".LoadAs<MineFieldView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;
        view.PlayerColor = player?.Color ?? Color.clear;

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
    public static void Place(string playerColor, Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null) {
            var player = level.players.Find(player => player.ColorName.ToString() == playerColor);
            if (level.mineFields.TryGetValue(position, out var oldMineField)) {
                oldMineField.Dematerialize();
                level.mineFields.Remove(position);
            }
            var mineField = new MineField {
                level = level,
                position = position,
                player = player
            };
            level.mineFields[position] = mineField;
            mineField.Materialize();
        }
    }
    [Command]
    public static void Remove(Vector2Int position) {
        var level = Game.Instance.TryGetLevel;
        if (level != null && level.mineFields.TryGetValue(position, out var mineField)) {
            mineField.Dematerialize();
            level.mineFields.Remove(position);
        }
    }
}