using System;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(TransformList))]
public class TransformList : ScriptableObject {
	public Matrix4x4[] matrices = Array.Empty<Matrix4x4>();
	public Vector3 min, max;
}