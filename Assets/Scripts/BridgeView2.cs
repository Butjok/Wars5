using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BridgeView2 : MonoBehaviour {

    public Mesh meshEnd;
    public Mesh meshTile;

    public Material material;
    public Dictionary<Vector2Int, (MeshFilter meshFilter, MeshCollider meshCollider)> pieces = new();

    public int rotationOffset;
    public int? Hp { get; set; }

    public void SetPositions(List<Vector2Int> positions) {
        Assert.IsTrue(positions.Count > 0);
        for (var i = 1; i < positions.Count - 1; i++)
            Assert.IsTrue(positions[i + 1] - positions[i] == positions[1] - positions[0]);

        var unused = pieces.Keys.ToHashSet();
        for (var i = 0; i < positions.Count; i++) {
            var position = positions[i];
            if (!pieces.TryGetValue(position, out var piece)) {
                var go = new GameObject("BridgePiece");
                go.transform.SetParent(transform);
                go.transform.position = position.ToVector3();
                go.layer = LayerMask.NameToLayer("Roads");
                var meshFilter = go.AddComponent<MeshFilter>();
                var meshCollider = go.AddComponent<MeshCollider>();
                piece = (meshFilter, meshCollider);
                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                pieces.Add(position, piece);
            }
            var isLastPiece = i == positions.Count - 1;
            piece.meshFilter.sharedMesh = i == 0 || isLastPiece ? meshEnd : meshTile;
            piece.meshCollider.sharedMesh = piece.meshFilter.sharedMesh;
            if (positions.Count > 1)
                piece.meshFilter.transform.rotation = Quaternion.LookRotation((positions[1] - positions[0]).Rotate(rotationOffset + (isLastPiece ? 2 : 0)).ToVector3(), Vector3.up);
            unused.Remove(position);
        }
        foreach (var position in unused) {
            Destroy(pieces[position].meshFilter.gameObject);
            pieces.Remove(position);
        }
    }

    public void OnGUI() {
        if (Hp is {} actualValue && pieces.Count > 0) {
            GUI.skin = DefaultGuiSkin.TryGet;
            var text = actualValue.ToString();
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            var accumulator = Vector2.zero;
            foreach (var piece in pieces.Values)
                accumulator +=   piece.meshFilter.transform.position.ToVector2();
            accumulator /= pieces.Count;
            var position = Camera.main. WorldToScreenPoint(accumulator.ToVector3());
            GUI.Label(new Rect(position.x - size.x / 2, Screen.height - position.y - size.y / 2, size.x, size.y), text);
        }
    }
}