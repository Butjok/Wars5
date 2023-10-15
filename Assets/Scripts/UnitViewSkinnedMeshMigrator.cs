using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(UnitView))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class UnitViewSkinnedMeshMigrator : MonoBehaviour {

    public UnitView unitView;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Mesh mesh;
    
    public void Reset() {
        unitView = GetComponent<UnitView>();
        Assert.IsTrue(unitView);
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Assert.IsTrue(skinnedMeshRenderer);
    }

    private static Dictionary<UnitView, Mesh> combinedMeshes = new();

    private static List<Matrix4x4> bindPoses = new();
    private static List<Vector3> vertices = new();
    private static List<int> triangles = new();
    private static List<BoneWeight> boneWeights = new();

    public void Migrate() {
        
        var oldPosition = transform.position;
        var oldRotation = transform.rotation;
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        var bones = new List<Transform>();

        var meshExists = combinedMeshes.TryGetValue(unitView.prefab, out var mesh) && mesh; 
        if (!meshExists) {
            mesh = new Mesh();
            mesh.name = Guid.NewGuid().ToString();
        }

        bindPoses.Clear();
        vertices.Clear();
        triangles.Clear();
        boneWeights.Clear();

        Transform body;
        
        // body
        {
            var go = new GameObject("Body");
            var bone = go.transform;
            body = bone;
            bone.SetParent(transform);
            bones.Add(bone);
            bone.SetPositionAndRotation(unitView.body.position, unitView.body.rotation);
            
            if (!meshExists) {
                bindPoses.Add(bone.worldToLocalMatrix * transform.localToWorldMatrix);

                var vertexStartIndex = vertices.Count;
                var bodyMesh = unitView.body.GetComponent<MeshFilter>().sharedMesh;
                vertices.AddRange(bodyMesh.vertices.Select(bone.localToWorldMatrix.MultiplyPoint));
                triangles.AddRange(bodyMesh.triangles.Select(index => index + vertexStartIndex));
                boneWeights.AddRange(Enumerable.Repeat(new BoneWeight { boneIndex0 = 0, weight0 = 1 }, bodyMesh.vertexCount));
            }
            
            unitView.body.gameObject.SetActive(false);
            unitView.body = bone;
        }
        
        // wheels
        foreach (var wheel in unitView.wheels) {
            
            var go = new GameObject("Wheel");
            var bone = go.transform;
            bone.SetParent(transform);
            bones.Add(bone);
            bone.SetPositionAndRotation(wheel.transform.position, wheel.transform.rotation);

            if (!meshExists) {
                bindPoses.Add(bone.worldToLocalMatrix * transform.localToWorldMatrix);

                var vertexStartIndex = vertices.Count;
                var wheelMesh = wheel.transform.GetComponent<MeshFilter>().sharedMesh;
                vertices.AddRange(wheelMesh.vertices.Select(bone.localToWorldMatrix.MultiplyPoint));
                triangles.AddRange(wheelMesh.triangles.Select(index => index + vertexStartIndex));
                boneWeights.AddRange(Enumerable.Repeat(new BoneWeight { boneIndex0 = bones.Count - 1, weight0 = 1 }, wheelMesh.vertexCount));
            }

            wheel.transform.gameObject.SetActive(false);
            wheel.transform = bone;
        }
        
        // turrets
        foreach (var turret in unitView.turrets) {
            
            var go = new GameObject("Turret");
            var bone = go.transform;
            bone.SetParent(body);
            bones.Add(bone);
            bone.SetPositionAndRotation(turret.transform.position, turret.transform.rotation);

            if (!meshExists) {
                bindPoses.Add(bone.worldToLocalMatrix * transform.localToWorldMatrix);

                var vertexStartIndex = vertices.Count;
                var turretMesh = turret.transform.GetComponent<MeshFilter>().sharedMesh;
                vertices.AddRange(turretMesh.vertices.Select(bone.localToWorldMatrix.MultiplyPoint));
                triangles.AddRange(turretMesh.triangles.Select(index => index + vertexStartIndex));
                boneWeights.AddRange(Enumerable.Repeat(new BoneWeight { boneIndex0 = bones.Count - 1, weight0 = 1 }, turretMesh.vertexCount));
            }

            turret.transform.gameObject.SetActive(false);
            turret.transform = bone;
            
            // barrels
            foreach (var barrel in turret.barrels) {
                
                var barrelGo = new GameObject("Barrel");
                var barrelBone = barrelGo.transform;
                barrelBone.SetParent(turret.transform);
                bones.Add(barrelBone);
                barrelBone.SetPositionAndRotation(barrel.transform.position, barrel.transform.rotation);

                if (!meshExists) {
                    bindPoses.Add(barrelBone.worldToLocalMatrix * barrel.transform.localToWorldMatrix);

                    var barrelVertexStartIndex = vertices.Count;
                    var barrelMesh = barrel.transform.GetComponent<MeshFilter>().sharedMesh;
                    vertices.AddRange(barrelMesh.vertices.Select(vertex => (barrelBone.worldToLocalMatrix * barrel.transform.localToWorldMatrix).MultiplyPoint(vertex)));
                    triangles.AddRange(barrelMesh.triangles.Select(index => index + barrelVertexStartIndex));
                    boneWeights.AddRange(Enumerable.Repeat(new BoneWeight { boneIndex0 = bones.Count - 1, weight0 = 1 }, barrelMesh.vertexCount));
                }

                barrel.transform.gameObject.SetActive(false);
                barrel.transform = barrelBone;
            }
        }

        if (!meshExists) {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.boneWeights = boneWeights.ToArray();
            mesh.bindposes = bindPoses.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            combinedMeshes[unitView.prefab] = mesh;
        }
        
        skinnedMeshRenderer.sharedMesh = mesh;
        skinnedMeshRenderer.bones = bones.ToArray();
        skinnedMeshRenderer.rootBone = bones[0];
        
        transform.position = oldPosition;
        transform.rotation = oldRotation;
    }
}