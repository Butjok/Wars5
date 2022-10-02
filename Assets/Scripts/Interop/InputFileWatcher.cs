using System;
using System.IO;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class InputFileWatcher : MonoBehaviour {

    public string path = "";
    public FileSystemWatcher watcher;
    public Game2 game;

    public void Start() {
        Assert.IsTrue(game);
        watcher = CreateFileWatcher(path);
    }
    private void OnDestroy() {
        watcher?.Dispose();
    }

    public FileSystemWatcher CreateFileWatcher(string path) {

        var watcher = new FileSystemWatcher();
        watcher.Path = Path.GetDirectoryName(path);
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                        | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Filter = Path.GetFileName(path);

        watcher.Changed += (_, args) => {
            var input = File.ReadAllText(args.FullPath);
            try {
                Interpreter.Execute(input);
            }
            catch (Exception e) {
                Debug.LogError($"{input}\n\n{e}", this);
            }
        };
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    public Unit unit;
    
    [Command]
    public void Move(Vector2Int from, Vector2Int to) {
        Assert.IsTrue(game.TryGetUnit(from,out var unit));
        unit.position.v = to;
    }
    [Command]
    public void Stay() {
        unit.moved.v = true;
    }
    [Command]
    public void Attack(Vector2Int position) {
        unit.moved.v = true;
    }
    [Command]
    public void Supply(Vector2Int position) {
        unit.moved.v = true;
    }
    [Command]
    public void Build(Vector2Int position, string typeName) {
        Assert.IsTrue(game.TryGetBuilding(position, out var building));
        Assert.IsTrue(building.player.v!=null);
        Assert.IsTrue(!game.TryGetUnit(position,out _));
        var type = Enum.Parse<UnitType>(typeName);
        Assert.IsTrue((Rules.BuildableUnits(building.type) & type) != 0);
        game.units[building.position] = new Unit(building.player.v, type: type, position: position);
    }
}