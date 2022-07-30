using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = nameof(Foliage))]
public class Foliage : ScriptableObject {

	[Serializable]
	public struct Type {
		public Mesh mesh;
		public Matrix4x4[] transforms;
		public Material[] materials;
	}

	public const string prefix = "foliage:";

	public Transform transform;
	[FormerlySerializedAs("entries")] public List<Type> types = new();

	[ContextMenu(nameof(Initialize))]
	public void Initialize() {

		if (!transform)
			return;

		var result = new Dictionary<Mesh, List<Matrix4x4>>();
		Initialize(transform, result);
		
		types.Clear();
		foreach (var (mesh, list) in result)
			types.Add(new Type{mesh=mesh,transforms=list.ToArray()});
	}

	public void Initialize(Transform transform, Dictionary<Mesh, List<Matrix4x4>> result) {
		for (var i = 0; i < transform.childCount; i++) {

			var child = transform.GetChild(i);
			Initialize(child, result);

			if (!child.name.StartsWith(prefix))
				continue;

			var start = prefix.Length;
			var end = child.name.Length;
			var dotIndex = child.name.LastIndexOf('.');
			if (dotIndex != -1)
				end = dotIndex;
			var foliageName = child.name.Substring(start, end - start);

			var mesh = Resources.Load<Mesh>(foliageName);
			if (!mesh) {
				Debug.LogWarning($"Foliage mesh '{foliageName}' could not be loaded.");
				continue;
			}

			if (!result.TryGetValue(mesh, out var list))
				list = result[mesh] = new List<Matrix4x4>();
			list.Add(child.localToWorldMatrix);
		}
	}
}