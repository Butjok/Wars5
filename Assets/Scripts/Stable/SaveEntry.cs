using System;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using UnityEngine;

public class SaveEntry {

    public string fileName;
    public string name;
    public DateTime dateTime;
    public string sceneName;
    public string commands;

    // static

    [Command]
    public static string DirectoryPath => Path.Combine(Application.persistentDataPath, "Saves");
    public static string GetFilePath(string fileName) {
        return Path.Combine(DirectoryPath, fileName);
    }
    public const string extension = ".json";
    public const string screenshotExtension = ".png";

    public static IEnumerable<string> FileNames
        => !Directory.Exists(DirectoryPath) ? Array.Empty<string>() : Directory.GetFiles(DirectoryPath, "*" + extension);

    public static SaveEntry Read(string fileName) {
        var path = GetFilePath(fileName);
        if (!File.Exists(path))
            return null;
        var json = File.ReadAllText(path);
        var entry = json.FromJson<SaveEntry>();
        entry.fileName = fileName;
        return entry;
    }

    public static Sprite LoadScreenshot(string fileName) {
        fileName = Path.ChangeExtension(fileName, screenshotExtension);
        var path = GetFilePath(fileName);
        if (!File.Exists(path))
            return null;
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path), true);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    public static string Save(SaveEntry entry, string fileName = null) {

        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        if (fileName == null)
            do
                fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);
            while (File.Exists(GetFilePath(fileName)));

        var path = GetFilePath(fileName);
        File.WriteAllText(path, entry.ToJson());
        return fileName;
    }

    [Command]
    public static void SaveRandomEntry() {
        var fileName = Save(new SaveEntry {
            dateTime = DateTime.Now,
            name = "Hello!",
            sceneName = "SampleScene",
            commands = "2 2 int2 position"
        });
        Debug.Log($"Saved into {fileName}");
    }
}