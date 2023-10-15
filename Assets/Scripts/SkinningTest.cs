using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SkinningTest : MonoBehaviour {

    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Mesh partMesh;
    public Mesh mesh;
    public List<Transform> bones = new();
    public int count = 3;

    public void Awake() {

        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Assert.IsTrue(skinnedMeshRenderer);
        mesh = new Mesh();
        var bindPoses = new List<Matrix4x4>();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var boneWeights = new List<BoneWeight>();

        var combineMeshInstances = new List<CombineInstance>();
        
        for (var i = 0; i < count; i++) {

            var go = new GameObject($"Bone{i}");
            var bone = go.transform;
            bone.SetParent(transform);
            bones.Add(bone);
            bone.position = Random.onUnitSphere * 5;

            bindPoses.Add(bone.worldToLocalMatrix * transform.localToWorldMatrix);
            
            var vertexStartIndex = vertices.Count;
            vertices.AddRange(partMesh.vertices.Select(bone.localToWorldMatrix.MultiplyPoint));
            triangles.AddRange(partMesh.triangles.Select(index => index + vertexStartIndex));
            boneWeights.AddRange(Enumerable.Repeat(new BoneWeight { boneIndex0 = i, weight0 = 1 }, partMesh.vertexCount));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.boneWeights = boneWeights.ToArray();
        
        mesh.bindposes = bindPoses.ToArray();
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        
        skinnedMeshRenderer.sharedMesh = mesh;
        skinnedMeshRenderer.bones = bones.ToArray();
        skinnedMeshRenderer.rootBone = bones[0];
    }
}