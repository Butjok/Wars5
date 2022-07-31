using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FoliageRenderer : MonoBehaviour {
	public Foliage foliage;
	public const int batch = 512;
	public void Update() {
		if (!foliage)
			return;
		foreach (var entry in foliage.types)
			for (var i = 0; i < entry.mesh.subMeshCount; i++)
			for (var skip = 0; skip < entry.transforms.Length; skip += batch) {
				var matrix4X4s = entry.transforms.Skip(skip).Take(Mathf.Min(batch, entry.transforms.Length - skip)).ToList();
				Graphics.DrawMeshInstanced(entry.mesh, i, entry.materials[i],
					matrix4X4s);
			}
	}
}