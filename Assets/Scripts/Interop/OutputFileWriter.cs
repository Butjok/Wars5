using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class OutputFileWriter : MonoBehaviour {
    
    public string path = "";
    public Game2 game;
    
    [ContextMenu(nameof(Write))]
    public void Write() {
        Assert.IsTrue(game);
        File.WriteAllText(path, new SerializedGame(game).ToJson());
    }
}