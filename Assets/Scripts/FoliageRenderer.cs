using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class FoliageRenderer : MonoBehaviour {
	public Foliage foliage;
	public const int batch = 512;
	public void Update() {
		if (!foliage)
			return;
		foreach (var entry in foliage.types)
			for (var i = 0; i < entry.mesh.subMeshCount; i++)
			for (var skip = 0; skip < entry.transforms.Length; skip += batch)
				Graphics.DrawMeshInstanced(entry.mesh, i, entry.materials[i],
					entry.transforms.Skip(skip).Take(Mathf.Min(batch, entry.transforms.Length - skip)).ToArray());
	}
}