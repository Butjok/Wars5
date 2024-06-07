using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class InstancedMeshRenderer3 : MonoBehaviour {

    public Mesh mesh;
    public Material[] materials = { };
    public string transformsUniformName = "_Transforms";
    public string inverseTransformsUniformName = "_InverseTransforms";
    public string colorsUniformName = "_Colors";
    public string indexOffsetUniformName = "_IndexOffset";

    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

    public List<Matrix4x4> transforms = new();
    private ComputeBuffer transformsBuffer;
    private ComputeBuffer inverseTransformsBuffer;
    private ComputeBuffer colorsBuffer;
    private Stack<ComputeBuffer> argsBufferPool = new();
    private Stack<ComputeBuffer> usedArgsBuffers = new();
    private uint[] uintArray = new uint[5];
    private Bounds bounds;

    public virtual void Update() {
        DrawInstances(0, 0, transforms.Count);
    }

    public virtual void Clear() {
        transformsBuffer?.Release();
        transformsBuffer = null;
        inverseTransformsBuffer?.Release();
        inverseTransformsBuffer = null;
        colorsBuffer?.Release();
        colorsBuffer = null;

        foreach (var argsBuffer in usedArgsBuffers)
            argsBuffer.Release();
        usedArgsBuffers.Clear();
        foreach (var argsBuffer in argsBufferPool)
            argsBuffer.Release();
        argsBufferPool.Clear();

        foreach (var material in materials) {
            material.SetBuffer(transformsUniformName, transformsBuffer);
            material.SetBuffer(inverseTransformsUniformName, inverseTransformsBuffer);
            material.SetBuffer(colorsUniformName, colorsBuffer);
        }
    }

    public virtual void SetTransforms(List<Matrix4x4> transforms) {
        this.transforms = transforms;

        if (transforms.Count == 0) {
            Clear();
            return;
        }

        transformsBuffer?.Release();
        transformsBuffer = null;
        inverseTransformsBuffer?.Release();
        inverseTransformsBuffer = null;

        bounds = new Bounds();
        foreach (var transform in transforms) {
            var position = transform.GetColumn(3);
            bounds.Encapsulate(position);
        }

        if (transforms.Count > 0) {
            transformsBuffer = new ComputeBuffer(transforms.Count, sizeof(float) * 16);
            transformsBuffer.SetData(transforms);

            inverseTransformsBuffer = new ComputeBuffer(transforms.Count, sizeof(float) * 16);
            inverseTransformsBuffer.SetData(transforms.Select(t => t.inverse).ToArray());
        }

        foreach (var material in materials) {
            material.SetBuffer(transformsUniformName, transformsBuffer);
            material.SetBuffer(inverseTransformsUniformName, inverseTransformsBuffer);
        }
    }

    public void SetColors(List<Color> colors) {
        colorsBuffer?.Release();
        colorsBuffer = null;
        if (colors.Count > 0) {
            colorsBuffer = new ComputeBuffer(colors.Count, sizeof(float) * 4);
            colorsBuffer.SetData(colors);
        }
        foreach (var material in materials)
            material.SetBuffer(colorsUniformName, colorsBuffer);
    }

    public int lastFrame = -1;

    public void DrawInstances(int submeshIndex, int startIndex, int instanceCount) {
        if (transformsBuffer == null || inverseTransformsBuffer == null)
            return;

        if (Time.frameCount != lastFrame) {
            lastFrame = Time.frameCount;
            foreach (var usedArgsBuffer in usedArgsBuffers)
                argsBufferPool.Push(usedArgsBuffer);
            usedArgsBuffers.Clear();
        }

        if (argsBufferPool.Count == 0)
            argsBufferPool.Push(new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments));
        var argsBuffer = argsBufferPool.Pop();
        usedArgsBuffers.Push(argsBuffer);

        uintArray[0] = mesh.GetIndexCount(submeshIndex);
        uintArray[1] = (uint)instanceCount;
        uintArray[2] = mesh.GetIndexStart(submeshIndex);
        uintArray[3] = mesh.GetBaseVertex(submeshIndex);
        uintArray[4] = 0;
        argsBuffer.SetData(uintArray);

        var properties = new MaterialPropertyBlock();
        properties.SetInt(indexOffsetUniformName, startIndex);
        Graphics.DrawMeshInstancedIndirect(
            mesh,
            submeshIndex,
            materials[submeshIndex],
            bounds,
            argsBuffer, properties: properties,
            castShadows: shadowCastingMode,
            receiveShadows: true,
            lightProbeUsage: lightProbeUsage,
            layer: gameObject.layer);
    }

    public void OnDestroy() {
        Clear();
    }
}