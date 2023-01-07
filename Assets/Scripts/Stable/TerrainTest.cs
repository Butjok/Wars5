using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class TerrainTest : MonoBehaviour {

    public Texture2D texture;
    public MaterialPropertyBlock materialPropertyBlock;
    public Renderer targetRenderer;
    public string textureName = "_MainTex";
    public string boundsName = "_Bounds";

    public List<Vector2Int> positions = new() {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(2, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(0, 2),
    };
    public Vector2Int startPosition = Vector2Int.zero;
    public Vector2Int[] offsets = {
        Vector2Int.right, 
        Vector2Int.left, 
        Vector2Int.up, 
        Vector2Int.down, 
    };

    public static readonly Vector2Int padding = Vector2Int.one;

    private Texture2D CreateTexture(IEnumerable<Vector2Int> positions, out Vector2Int min, out Vector2Int max) {

        var set =new HashSet<Vector2Int>(positions);
        Assert.AreNotEqual(0, set.Count);

        min = new Vector2Int(set.Select(p => p.x).Min(), set.Select(p => p.y).Min());
        max = new Vector2Int(set.Select(p => p.x).Max(), set.Select(p => p.y).Max());
        
        min -= padding;
        max += padding;
        
        var size = max - min + new Vector2Int(1, 1);

        var texture = new Texture2D(size.x, size.y, TextureFormat.RFloat, false, true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (var y = 0; y < texture.height; y++)
        for (var x = 0; x < texture.width; x++)
            texture.SetPixel(x, y, new Color(-1,0,0,0));
        texture.Apply();

        var distances = new Dictionary<Vector2Int, int> {
            [startPosition] = 0
        };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startPosition);
        while (queue.Count > 0) {
            var position = queue.Dequeue();
            foreach (var offset in offsets) {
                var neighborPosition = position + offset;
                if (!set.Contains(neighborPosition) || distances.ContainsKey(neighborPosition))
                    continue;
                distances[neighborPosition] = distances[position] + 1;
                queue.Enqueue(neighborPosition);
            }
        }
        
        foreach (var position in set) {
            var localPosition = position - min;
            var distance = distances.TryGetValue(position, out var d) ? d : -2;
            texture.SetPixel(localPosition.x, localPosition.y, new Color(distance, 0, 0, 0));
        }
        texture.Apply();

        return texture;
    }

    [Command]
    public void Clear() {
        positions.Clear();
        if (texture) {
            Destroy(texture);
            texture = null;
        }
        SetUniforms(null);
    }

    [Command]
    public void AddPosition(Vector2Int position) {
        positions.Add(position);
        if (texture)
            Destroy(texture);
        texture = CreateTexture(positions, out var min, out var max);
        SetUniforms(texture, min, max);
    }

    [Command]
    public void RemovePosition(Vector2Int position) {
        if (positions.RemoveAll(item => item == position) == 0)
            return;
        if (texture)
            Destroy(texture);
        texture = CreateTexture(positions, out var min, out var max);
        SetUniforms(texture, min, max);
    }

    private void SetUniforms(Texture texture, Vector2Int min = default, Vector2Int max = default) {
        materialPropertyBlock ??= new MaterialPropertyBlock();
        materialPropertyBlock.SetVector(boundsName, new Vector4(min.x, min.y, max.x, max.y));
        materialPropertyBlock.SetTexture(textureName, texture);
        materialPropertyBlock.SetFloat("_WaveStartTime", Time.timeSinceLevelLoad);
        targetRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}