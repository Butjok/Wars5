using UnityEngine;

[ExecuteInEditMode]
public class TileMapTester : MonoBehaviour {

    public Material material;

    public void Update() {
        var matrix = transform.worldToLocalMatrix;
        material.SetMatrix("_WorldToTileMap", matrix);
    }
}