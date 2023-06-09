using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class InputCommandsListener : MonoBehaviour {

    // public string inputPath = "";
    // public string outputPath = "";
    // public DateTime? lastWriteTime;
    // public Level level;
    // public bool executeOnStart = true;
    //
    // private void Awake() {
    //     level = GetComponent<Level>();
    //     Assert.IsTrue(level);
    // }
    //
    // private void Start() {
    //     if (!executeOnStart)
    //         lastWriteTime = File.GetLastWriteTime(inputPath);
    // }
    //
    // private void Update() {
    //     var lastWriteTime = File.GetLastWriteTime(inputPath);
    //     if (lastWriteTime != this.lastWriteTime) {
    //         this.lastWriteTime = lastWriteTime;
    //         level.commands.Enqueue(File.ReadAllText(inputPath));
    //     }
    // }
}