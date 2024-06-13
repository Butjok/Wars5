using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class InstancedMeshRenderer2 : MonoBehaviour {

    public Mesh mesh;
    public Material[] materials = { };
    public string transformsUniformName = "_Transforms";

    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

    public List<Matrix4x4> transforms = new();
    public Bounds? bounds;
    public float radius = 1f;
    public ComputeBuffer transformsBuffer;
    public List<ComputeBuffer> argsBuffers = new();

    public void Update() {

        /*if (!mesh || transforms.Count == 0)
            return;

        if (transformsBuffer == null || argsBuffers.Count == 0)
            UpdateGpuData();

        if (bounds == null)
            RecalculateBounds();

        for (var i = 0; i < mesh.subMeshCount; i++)
            if (materials[i])
                Graphics.DrawMeshInstancedIndirect(mesh, i, materials[i], (Bounds)bounds, argsBuffers[i], 0, null, layer: gameObject.layer, castShadows: shadowCastingMode,
                    lightProbeUsage: lightProbeUsage);*/
    }

    public static Bounds CalculateBounds(IReadOnlyCollection<Matrix4x4> transforms, float radius) {
        // return a Bounds located at zero which encapsulates all the transforms
        /*var max = Vector3.zero;
        foreach (var transform in transforms) {
            Vector3 position = transform.GetColumn(3);
            max = Vector3.Max(max, (position.Abs() + Vector3.one * radius));
        }
        return new Bounds(Vector3.zero, max * 2);*/
        return default;
    }
    public void RecalculateBounds() {
        //bounds = CalculateBounds(transforms, radius);
    }

    public void UpdateGpuData() {

        /*ReleaseGpuData();

        if (transforms.Count == 0)
            return;

        transformsBuffer = new ComputeBuffer(transforms.Count, sizeof(float) * 16);
        transformsBuffer.SetData(transforms);

        for (var i = 0; i < mesh.subMeshCount; i++) {
            var argsBuffer = new ComputeBuffer(1, sizeof(int) * 5, ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(new[] { (int)mesh.GetIndexCount(i), transforms.Count, (int)mesh.GetIndexStart(i), (int)mesh.GetBaseVertex(i), 0 });
            argsBuffers.Add(argsBuffer);
        }

        foreach (var material in materials)
            material.SetBuffer(transformsUniformName, transformsBuffer);*/
    }

    public void ReleaseGpuData() {
        /*transformsBuffer?.Release();
        transformsBuffer = null;
        foreach (var argsBuffer in argsBuffers)
            argsBuffer?.Release();
        argsBuffers.Clear();*/
    }

    public void OnEnable() {
        //UpdateGpuData();
    }
    public void OnDisable() {
        //ReleaseGpuData();
    }
}