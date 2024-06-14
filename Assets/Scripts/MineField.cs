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

    public void Explode(Unit unit = null) {
        if (unit == null)
            foreach (var u in level.units.Values)
                if (u.view.Position == position) {
                    unit = u;
                    break;
                }
        
        if (unit != null && unit.Hp > 0) {
            var newUnitHp = unit.Hp - Rules.MineFieldDamage(unit, this);
            if (newUnitHp <= 0 && unit.view.Position != position) {
                unit.view.Position = position;
                unit.view.PlaceOnTerrain(true);
            }

            unit.view.TriggerDamageFlash();
            unit.view.ApplyDamageTorque(UnitView.DamageTorqueType.MissileExplosion);

            unit.SetHp(newUnitHp, true);
        }

        level.view.cameraRig.Shake();
        Effects.SpawnExplosion(position.Raycasted(), Vector3.up, parent: level.view.transform);
        ExplosionCrater.SpawnDecal(position, parent: level.view.transform);
        Sounds.PlayOneShot(Sounds.explosion);

        level.mineFields.Remove(position);
        Despawn();
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