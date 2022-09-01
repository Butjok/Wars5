using UnityEngine;

[ExecuteInEditMode]
public class InstancedMeshRenderer : MonoBehaviour {

	public TransformList transformList;
	public Mesh mesh;
	public Material[] materials = { };

	public MaterialPropertyBlock materialPropertyBlock;

	public ComputeBuffer buffer;

	public void Update() {

		if (!transformList || !mesh || transformList.matrices.Length == 0)
			return;

		if (mesh.subMeshCount != materials.Length) {
			Debug.LogWarning($"Submeshes != material: {mesh.subMeshCount} != {materials.Length}");
			return;
		}

		if (materialPropertyBlock == null) {
			
			buffer?.Release();
			buffer = new ComputeBuffer(transformList.matrices.Length, sizeof(float) * 16);
			buffer.SetData(transformList.matrices);

			materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetBuffer("_Transforms", buffer);
		}

		for (var i = 0; i < mesh.subMeshCount; i++) {
			if (!materials[i]) {
				Debug.LogWarning($"Empty material for submesh {i}.", this);
				continue;
			}
			Graphics.DrawMeshInstancedProcedural(mesh, i, materials[i], transformList.bounds, transformList.matrices.Length, materialPropertyBlock);
		}
	}

	public void OnDestroy() {
		buffer?.Release();
	}
}