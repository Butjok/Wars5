using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LevelEditorFileSystem {

    public static string SaveRootDirectoryPath => Path.Combine(Application.dataPath, "Saves");
    public static string GetSavePath(string name) => Path.Combine(SaveRootDirectoryPath, name);

    public static void Save(string name, Level level) {
        using var stringWriter = new StringWriter();
        new LevelWriter(stringWriter).WriteLevel(level);
        var text = stringWriter.ToString();
        Save(name, text);
    }

    public static void Save(string name, string text) {
        if (!Directory.Exists(SaveRootDirectoryPath))
            Directory.CreateDirectory(SaveRootDirectoryPath);
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var saveName = name + "-" + DateTime.Now.ToString("G", CultureInfo.GetCultureInfo("de-DE")).Replace(":", ".").Replace(" ", "-") + ".save";
        var filePath = Path.Combine(path, saveName);
        File.WriteAllText(filePath, text);

        Debug.Log($"Saved to: {filePath}");
    }

    public static string TryReadLatest(string name) {
        return TryGetLatestPath(name, out var path) ? File.ReadAllText(path) : null;
    }

    public static int DeleteAutosaves(Predicate<string> predicate) {
        var count = 0;
        foreach (var filePath in GetPaths("autosave").ToArray()) {
            if (predicate(filePath)) {
                File.Delete(filePath);
                if (File.Exists(filePath + ".meta"))
                    File.Delete(filePath + ".meta");
                count++;
            }
        }
        return count;
    }
    public static int DeleteOldAutosaves(int autosaveLifespanInDays) {
        return DeleteAutosaves(path => {
            var lastAccessTime = File.GetLastWriteTime(path);
            return DateTime.Now.Subtract(lastAccessTime).Days > autosaveLifespanInDays;
        });
    }
    public static int DeleteAllAutosaves() {
        return DeleteAutosaves(_ => true);
    }

    public static bool TryGetLatestPath(string name, out string filePath) {
        filePath = default;
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            return false;
        var files = GetPaths(name).ToArray();
        if (files.Length == 0)
            return false;
        filePath = files.OrderBy(File.GetLastWriteTime).Last();
        return true;
    }

    public static IEnumerable<string> GetPaths(string name) {
        var path = GetSavePath(name);
        if (!Directory.Exists(path))
            return Enumerable.Empty<string>();
        return Directory.GetFiles(path).Where(p => p.EndsWith(".txt") || p.EndsWith(".save"));
    }
}