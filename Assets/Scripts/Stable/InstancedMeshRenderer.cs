using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InstancedMeshRenderer : MonoBehaviour {

    public TransformList transformList;
    public Mesh mesh;
    public Material[] materials = { };

    public ComputeBuffer transformsBuffer;
    public List<ComputeBuffer> argsBuffers = new();

    public void Update() {

        if (!transformList || !mesh || transformList.matrices.Length == 0)
            return;

        if (mesh.subMeshCount != materials.Length) {
            Debug.LogWarning($"Submeshes != material: {mesh.subMeshCount} != {materials.Length}");
            return;
        }

        if (transformsBuffer == null) {

            transformsBuffer = new ComputeBuffer(transformList.matrices.Length, sizeof(float) * 16);
            transformsBuffer.SetData(transformList.matrices);

            for (var i = 0; i < mesh.subMeshCount; i++) {
                var argsBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
                argsBuffer.SetData(new[] { (int)mesh.GetIndexCount(i), transformList.matrices.Length, (int)mesh.GetIndexStart(i),(int) mesh.GetBaseVertex(i), 0 });
                argsBuffers.Add(argsBuffer);
            }

            foreach (var material in materials)
                material.SetBuffer("_Transforms", transformsBuffer);
        }

        for (var i = 0; i < mesh.subMeshCount; i++) {
            if (!materials[i]) {
                Debug.LogWarning($"Empty material for submesh {i}.", this);
                continue;
            }
            Graphics.DrawMeshInstancedIndirect(mesh, i, materials[i], transformList.bounds, argsBuffers[i], 0, null, layer: gameObject.layer);
            //Graphics.DrawMeshInstancedProcedural(mesh, i, materials[i], transformList.bounds, transformList.matrices.Length, null, layer:gameObject.layer);
        }
    }

    private void OnEnable() {
        ResetGpuBuffers();
    }

    public void OnDisable() {
        ResetGpuBuffers();
    }

    public void OnDestroy() {
        ResetGpuBuffers();
    }

    public void ResetGpuBuffers() {
        transformsBuffer?.Release();
        transformsBuffer = null;
        foreach (var argsBuffer in argsBuffers)
            argsBuffer?.Release();
        argsBuffers.Clear();
    }
}