using System.Collections.Generic;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(BoxCollider))]
public class RegularGridInstancedMeshRenderer : InstancedMeshRenderer3 {

    public BoxCollider collider;
    public Vector3Int dimensions = Vector3Int.zero;
    public List<(int startIndex, List<Matrix4x4> transforms)> cells = new();
    public List<Matrix4x4> sortedTransforms = new();
    public Camera camera;
    public Vector3 desiredCellSize = Vector3.one;

    public void Reset() {
        collider = GetComponent<BoxCollider>();
        Assert.IsTrue(collider);
    }

    public bool InBounds(Vector3Int coordinates) {
        return coordinates.x >= 0 && coordinates.x < dimensions.x &&
               coordinates.y >= 0 && coordinates.y < dimensions.y &&
               coordinates.z >= 0 && coordinates.z < dimensions.z;
    }

    public Bounds GetCellBounds(Vector3Int coordinates) {
        var size = collider.size;
        var center = collider.center;
        var step = new Vector3(size.x / dimensions.x, size.y / dimensions.y, size.z / dimensions.z);
        var min = center - size / 2;
        var bounds = new Bounds();
        bounds.min = new Vector3(min.x + coordinates.x * step.x, min.y + coordinates.y * step.y, min.z + coordinates.z * step.z);
        bounds.max = bounds.min + step;
        return bounds;
    }

    public int GetCellFlatIndex(Vector3Int coordinates) {
        return coordinates.x + coordinates.y * dimensions.x + coordinates.z * dimensions.x * dimensions.y;
    }

    public override void Clear() {
        sortedTransforms.Clear();
        cells.Clear();
        dimensions = Vector3Int.zero;
        base.Clear();
    }

    public override void SetTransforms(List<Matrix4x4> transforms) {
        if (transforms.Count == 0) {
            Clear();
            return;
        }

        var bounds = new Bounds(transforms[0].GetColumn(3), Vector3.zero);
        foreach (var transform in transforms)
            bounds.Encapsulate(transform.GetColumn(3));
        collider.center = bounds.center;
        collider.size = bounds.size;
        dimensions = new Vector3Int(
            Mathf.Max(1, Mathf.CeilToInt(bounds.size.x / desiredCellSize.x)),
            Mathf.Max(1, Mathf.CeilToInt(bounds.size.y / desiredCellSize.y)),
            Mathf.Max(1, Mathf.CeilToInt(bounds.size.z / desiredCellSize.z))
        );

        cells.Clear();
        for (var i = 0; i < dimensions.x * dimensions.y * dimensions.z; i++)
            cells.Add((0, new List<Matrix4x4>()));

        var size = collider.size;
        var center = collider.center;
        var step = new Vector3(size.x / dimensions.x, size.y / dimensions.y, size.z / dimensions.z);
        var min = center - size / 2;

        foreach (var t in transforms) {
            var position = t.GetColumn(3);
            var coordinates = new Vector3Int(
                Mathf.FloorToInt((position.x - min.x) / step.x),
                Mathf.FloorToInt((position.y - min.y) / step.y),
                Mathf.FloorToInt((position.z - min.z) / step.z)
            );
            if (InBounds(coordinates))
                cells[GetCellFlatIndex(coordinates)].transforms.Add(t);
        }

        sortedTransforms.Clear();
        for (var i = 0; i < dimensions.x * dimensions.y * dimensions.z; i++) {
            var cell = cells[i];
            cell.startIndex = sortedTransforms.Count;
            sortedTransforms.AddRange(cell.transforms);
            cells[i] = cell;
        }
        base.SetTransforms(sortedTransforms);
    }

    [Command]
    public void PopulateInstances(int count) {
        var transforms = new List<Matrix4x4>();
        for (var i = 0; i < count; i++) {
            var x = Random.Range(0, 10f);
            var y = 0;
            var z = Random.Range(0f, 5f);
            var position = new Vector3(x, y, z);
            var rotation = Quaternion.Euler(180, Random.Range(0, 360), 0);
            var scale = Random.Range(0.5f, 1.5f);
            var transform = Matrix4x4.TRS(position, rotation, Vector3.one * scale);
            transforms.Add(transform);
        }
        SetTransforms(transforms);
    }

    public override void Update() {
        var rangesToDraw = new List<(int startIndex, int count)>();

        for (var x = 0; x < dimensions.x; x++)
        for (var y = 0; y < dimensions.y; y++)
        for (var z = 0; z < dimensions.z; z++) {
            var coordinates = new Vector3Int(x, y, z);
            var bounds = GetCellBounds(coordinates);
            if (!camera ||
                GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds)) {
                var flatIndex = GetCellFlatIndex(coordinates);
                var cell = cells[flatIndex];
                rangesToDraw.Add((cell.startIndex, cell.transforms.Count));
                DrawInstances(0, cell.startIndex, cell.transforms.Count);
            }
        }

        rangesToDraw.Sort((a, b) => a.startIndex.CompareTo(b.startIndex));
        var queue = new Queue<(int startIndex, int count)>(rangesToDraw);
        while (queue.TryDequeue(out var head)) {
            while (queue.TryPeek(out var next) && head.startIndex + head.count == next.startIndex) {
                head.count += next.count;
                queue.Dequeue();
            }
            //DrawInstances(0, head.startIndex, head.count);
        }
    }
}