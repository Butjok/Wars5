using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InputViewer : MonoBehaviour {

    public class Input {
        public List<SerializedVector2Int> points = new();
    }

    public TextAsset textAsset;
    public string path;
    public Input input = new();

    public void Start() {

        path = Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(textAsset));

        var watcher = new FileSystemWatcher();
        watcher.Path = Path.GetDirectoryName(path);
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                               NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Filter = Path.GetFileName(path);
        watcher.Changed += (_, args) => {
            Debug.Log("File: " + args.FullPath + " " + args.ChangeType);
            Parse();
        };
        watcher.EnableRaisingEvents = true;

        Parse();
    }

    public void Parse() {
        input = File.ReadAllText(path).FromJson<Input>();
    }

    public void OnDrawGizmos() {
        foreach (var point in input.points)
            Gizmos.DrawRay(((Vector2Int)point).ToVector3Int(), Vector3.up);
    }
}
