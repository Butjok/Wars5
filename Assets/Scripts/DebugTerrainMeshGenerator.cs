using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class DebugTerrainMeshGenerator : MonoBehaviour {

    public MeshFilter meshFilter;
    public Level level;

    public TileTypeColorDictionary colors = new() {
        [TileType.Plain] = Color.green,
        [TileType.Road] = Color.grey,
        [TileType.Sea] = Color.blue,
        [TileType.Mountain] = new Color(0.57f, 0.46f, 0.27f)
    };

    public void Generate() {
        Assert.IsTrue(level);
        meshFilter.sharedMesh = Generate(level);
    }

    public Mesh Generate(Level level) {

        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();

        foreach (var position in level.tiles.Keys) {

            Color color;
            
            var tileType = level.tiles[position];
            var building = level.buildings.TryGetValue(position, out var b) ? b : null;
            // if (building != null)
            //     color = building.Player?.Color ?? Color.white;
            // else {
            //     var found = this.colors.TryGetValue(tileType, out color);
            //     Assert.IsTrue(found, tileType.ToString());
            // }

            vertices.AddRange(MeshUtils.QuadAt(position.ToVector3Int()));
            for (var i = 0; i < 6; i++)
                triangles.Add(triangles.Count);
            // colors.AddRange(Enumerable.Repeat(color,6));
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