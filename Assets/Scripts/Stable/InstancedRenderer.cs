using System;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class InstancedRenderer : MonoBehaviour {

	public Material material;

	private ComputeBuffer meshPropertiesBuffer;
	private ComputeBuffer argsBuffer;

	private Bounds bounds;

	public Mesh mesh;
	public TransformList transformList;

	private struct MeshProperties {
		public Matrix4x4 mat;
		public Vector4 color;

		public static int Size() {
			return
				sizeof(float) * 4 * 4 + // matrix;
				sizeof(float) * 4; // color;
		}
	}

	private void Setup() {
		bounds = transformList.bounds;
		InitializeBuffers();
	}

	private void InitializeBuffers() {

		var population = transformList.matrices.Length;

		// Argument buffer used by DrawMeshInstancedIndirect.
		uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
		// Arguments for drawing mesh.
		// 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
		args[0] = (uint)mesh.GetIndexCount(0);
		args[1] = (uint)population;
		args[2] = (uint)mesh.GetIndexStart(0);
		args[3] = (uint)mesh.GetBaseVertex(0);
		argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		argsBuffer.SetData(args);

		// Initialize buffer with the given population.
		MeshProperties[] properties = new MeshProperties[population];
		for (int i = 0; i < population; i++) {
			MeshProperties props = new MeshProperties();
			Vector3 position = default; // new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
			Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
			Vector3 scale = Vector3.one;

			props.mat = Matrix4x4.TRS(position, rotation, scale);

			props.mat = transformList.matrices[i];
			props.color = Color.Lerp(Color.red, Color.blue, Random.value);

			properties[i] = props;
		}

		meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
		meshPropertiesBuffer.SetData(properties);
		material.SetBuffer("_Properties", meshPropertiesBuffer);
	}

	// Mesh Properties struct to be read from the GPU.
	// Size() is a convenience funciton which returns the stride of the struct.


	/*private Mesh CreateQuad(float width = 1f, float height = 1f) {
	    ...
	}*/

	private void OnEnable() {
		Setup();
	}

	private void OnDrawGizmosSelected() {
		if (transformList)
			foreach (var matrix in transformList.matrices) {
				var position = matrix * new Vector4(0,0,0,1);
				Gizmos.DrawWireSphere(position, .1f);
			}
	}

	private void Update() {
		
		if (argsBuffer ==null  || meshPropertiesBuffer == null || !material.HasBuffer("_Properties"))
			Setup();
		
		Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
	}

	private void OnDisable() {
		// Release gracefully.
		if (meshPropertiesBuffer != null) {
			meshPropertiesBuffer.Release();
		}
		meshPropertiesBuffer = null;

		if (argsBuffer != null) {
			argsBuffer.Release();
		}
		argsBuffer = null;
	}
}