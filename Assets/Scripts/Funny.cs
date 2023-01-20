using System.IO;
using Butjok.CommandLine;
using UnityEngine;

public static class Funny {

    public static string Path => System.IO.Path.Combine(Application.dataPath, "Funny.txt");
    
    [Command]
    public static string Message() {
        if (!File.Exists(Path))
            return null;
        return File.ReadAllLines(Path).Random();
    }
}