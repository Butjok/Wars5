using System;
using UnityEngine;

public class BuildingView : MonoBehaviour {
	public Building building;
	public MeshRenderer[] renderers = { };
	public MaterialPropertyBlock materialPropertyBlock;
	public void Start() {
		materialPropertyBlock = new MaterialPropertyBlock();
	}
	public void Reset() {
		renderers = GetComponents<MeshRenderer>();
	}
}