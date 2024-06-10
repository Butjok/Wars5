using System.Collections.Generic;
using Butjok.CommandLine;
using SaveGame;
using UnityEngine;
using UnityEngine.Assertions;

public class PipeSection : ISpawnable {
    public static readonly List<PipeSection> spawned = new();

    public Level level;
    public int hp = 10;
    public Vector2Int position;

    [DontSave] public PipeSectionView view;
    [DontSave] public bool IsSpawned { get; private set; }

    public void Spawn() {
        Assert.IsFalse(IsSpawned);
        Assert.IsFalse(spawned.Contains(this));
        spawned.Add(this);

        var prefab = "PipeSection".LoadAs<PipeSectionView>();
        Assert.IsTrue(prefab);
        view = Object.Instantiate(prefab, level.view.transform);
        view.Position = position;
        UpdateView();

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

    public void UpdateView() {
        var neighbors = new List<Vector2Int>(4);
        foreach (var offset in Rules.gridOffsets) {
            var neighborPosition = position + offset;
            if (level.TryGetPipeSection(neighborPosition, out _))
                neighbors.Add(neighborPosition);
        }
        switch (neighbors.Count) {
            case 1:
                view.Kind = PipeSectionKind.End;
                view.Forward = neighbors[0] - position;
                break;
            case 2:
                var toFirst = neighbors[0] - position;
                var toSecond = neighbors[1] - position;
                if (toFirst == -toSecond) {
                    view.Kind = PipeSectionKind.I;
                    view.Forward = toFirst;
                }
                else {
                    view.Kind = PipeSectionKind.L;
                    view.Forward = toFirst.Cross(toSecond) > 0 ? toFirst : toSecond;
                }
                break;
        }
    }

    [Command]
    public static bool TryRemovePipeSection(Vector2Int position) {
        return Game.Instance.TryGetLevel?.TryRemovePipeSection(position) ?? false;
    }
    [Command]
    public static bool TryAddPipeSection(Vector2Int position) {
        return Game.Instance.TryGetLevel?.TryAddPipeSection(position) ?? false;
    }
}