using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExampleClass : MonoBehaviour {
    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    public int startInstanceLocation = 0;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private int cachedStartInstanceLocation = -1;
    private ComputeBuffer transformsBuffer;
    private ComputeBuffer inverseTransformsBuffer;
    private ComputeBuffer argsBuffer;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Start() {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateTransforms();
        UpdateArgs();
    }

    void Update() {
        // Update starting position buffer
        //if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
          //  UpdateTransforms();

        if (cachedStartInstanceLocation != startInstanceLocation ||
            cachedInstanceCount != instanceCount)
            UpdateArgs();

        // Pad input
        if (Input.GetAxisRaw("Horizontal") != 0.0f)
            instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void OnGUI() {
        GUI.Label(new Rect(265, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }

    void UpdateTransforms() {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Positions
        if (transformsBuffer != null)
            transformsBuffer.Release();
        transformsBuffer = new ComputeBuffer(instanceCount, 16 * sizeof(float));
        List<Matrix4x4> transforms = new List<Matrix4x4>();
        for (int i = 0; i < instanceCount; i++) {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            var position = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
            transforms.Add (Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
        }
        transformsBuffer.SetData(transforms);
        
        inverseTransformsBuffer?. Release();
        inverseTransformsBuffer = new ComputeBuffer(instanceCount, 16 * sizeof(float));
        inverseTransformsBuffer.SetData( transforms.Select( t => t.inverse ).ToArray());
        
        instanceMaterial.SetBuffer("transforms", transformsBuffer);
        instanceMaterial.SetBuffer( "inverseTransforms", inverseTransformsBuffer);
        
        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void UpdateArgs() {
        // Indirect args
        if (instanceMesh != null) {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)Mathf.Clamp(instanceCount - startInstanceLocation, 0, instanceCount);
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            args[4] = (uint)startInstanceLocation;
        }
        else {
            args[0] = args[1] = args[2] = args[3] = args[4] = 0;
        }
        argsBuffer.SetData(args);
        
        cachedStartInstanceLocation = startInstanceLocation;
        cachedInstanceCount = instanceCount;
    }

    void OnDisable() {
        if (transformsBuffer != null)
            transformsBuffer.Release();
        transformsBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}