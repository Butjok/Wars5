using UnityEngine;

[ExecuteInEditMode]
public class FoliageRenderer : MonoBehaviour {
	public Foliage foliage;
	public void Update() {
		if (!foliage)
			return;
		foreach (var entry in foliage.entries)
			for (var i=0;i<entry.mesh.subMeshCount;i++)
				Graphics.DrawMeshInstanced(entry.mesh, i, entry.materials[i], entry.transforms);
	}
}