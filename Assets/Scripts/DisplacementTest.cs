using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;

public class DisplacementTest : MonoBehaviour {

    public MeshFilter meshFilter;
    public Vector2 size = new(10, 10);
    public Vector2Int count = new(50, 50);
    public List<GameObject> gameObjects = new();
    public Vector2 noiseScale = Vector2.one;
    public float noiseAmplitude = 1;
    public int octaves = 2;

    public void Awake() {
        Respawn();
    }

    [Command]
    public void Respawn() {
        foreach (var gameObject in gameObjects) Destroy(gameObject);
        gameObjects.Clear();

        var step = size / count;
        for (var y = 0; y < count.y; y++)
        for (var x = 0; x < count.x; x++) {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one / 10;
            cube.transform.position = (new Vector2Int(x, y) * step).ToVector3();
            gameObjects.Add(cube);
        }
    }

    public void Update() {
        foreach (var gameObject in gameObjects) {
            var position = gameObject.transform.position.ToVector2();
            var yOffset = 0f;
            var noiseScale = this.noiseScale;
            var noiseAmplitude = this.noiseAmplitude;
            for (var i = 0; i < octaves; i++) {
                yOffset += noiseAmplitude * Mathf.PerlinNoise(position.x / noiseScale.x, position.y / noiseScale.y);
                noiseScale /= 2;
                noiseAmplitude /= 2;
            }

            gameObject.transform.position = new Vector3(position.x, yOffset, position.y);
        }
    }
}