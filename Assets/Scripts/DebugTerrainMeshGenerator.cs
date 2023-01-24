using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class DebugTerrainMeshGenerator : MonoBehaviour {

    public MeshFilter meshFilter;
    public Main main;

    public TileTypeColorDictionary colors = new() {
        [TileType.Plain] = Color.green,
        [TileType.Road] = Color.grey,
        [TileType.Sea] = Color.blue,
        [TileType.Mountain] = new Color(0.57f, 0.46f, 0.27f)
    };

    public void Generate() {
        Assert.IsTrue(main);
        meshFilter.sharedMesh = Generate(main);
    }

    public Mesh Generate(Main main) {

        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();

        foreach (var position in main.tiles.Keys) {

            Color color;
            
            var tileType = main.tiles[position];
            var building = main.buildings.TryGetValue(position, out var b) ? b : null;
            if (building != null)
                color = building.Player?.color ?? Color.white;
            else {
                var found = this.colors.TryGetValue(tileType, out color);
                Assert.IsTrue(found, tileType.ToString());
            }

            vertices.AddRange(MeshUtils.QuadAt(position.ToVector3Int()));
            for (var i = 0; i < 6; i++)
                triangles.Add(triangles.Count);
            colors.AddRange(Enumerable.Repeat(color,6));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}

[Serializable]
public class TileTypeColorDictionary : SerializableDictionary<TileType, Color> { }