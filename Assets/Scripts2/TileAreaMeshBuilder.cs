using System.Collections.Generic;
using UnityEngine;

public class TileAreaMeshBuilder : MonoBehaviour {

    public Mesh quad;
    public Mesh subdividedQuad;

    public List<Vector2Int> positions=new ();
    public Mesh result;

    [ContextMenu(nameof(Test))]
    public void Test() {
        result = Build(positions);
        var filter = GetComponent<MeshFilter>();
        if (filter)
            filter.sharedMesh = result;
    }
    
    public  Mesh Build(IEnumerable<Vector2Int>_positions) {

        var positions = new HashSet<Vector2Int>(_positions);
        var border = new HashSet<Vector2Int>();

        foreach (var position in positions) {
            for (var yOffset =-1;yOffset <= 1; yOffset++)
            for (var xOffset = -1; xOffset <= 1; xOffset++)
                border.Add(position + new Vector2Int(xOffset, yOffset));
        }

        border.ExceptWith(positions);

        var subdividedQuadVertices = subdividedQuad.vertices;
        var subdividedQuadUvs = subdividedQuad.uv;
        var subdividedQuadTriangles = subdividedQuad.triangles;

        var quadVertices = quad.vertices;
        var quadUvs = quad.uv;
        var quadTriangles = quad.triangles;

        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        var colors = new List<Color>();

        void Add(Vector2Int position, Vector3[] pieceVertices, Vector2[] pieceUvs, int[] pieceTriangles) {
            
            var translate = Matrix4x4.Translate(position.ToVector3Int());
            var vertexStartIndex = vertices.Count;

            foreach (var vertex in pieceVertices) {
                Vector3 worldPosition = translate * vertex.ToVector4();
                vertices.Add(worldPosition);

                var minDistance = float.MaxValue;
                foreach (var p in positions)
                    minDistance = Mathf.Min(minDistance, (worldPosition.ToVector2() - p).SignedDistanceBox(Vector2.one / 2));
                
                colors.Add(new Color(minDistance,0,0));
            }
            foreach (var uv in pieceUvs)
                uvs.Add(uv);
            
            foreach (var triangle in pieceTriangles)
                triangles.Add(vertexStartIndex + triangle);
        }

        foreach (var position in positions)
            Add(position, quadVertices,quadUvs,quadTriangles);
        
        foreach (var position in border)
            Add(position, subdividedQuadVertices, subdividedQuadUvs, subdividedQuadTriangles);

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles= triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }
}