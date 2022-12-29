using System;
using UnityEngine;

public class BuildingView : MonoBehaviour {
	public Building building;
	public MeshRenderer[] renderers = { };
	public MaterialPropertyBlock materialPropertyBlock;
	public void Awake() {
		materialPropertyBlock = new MaterialPropertyBlock();
	}
	public void Reset() {
		renderers = GetComponents<MeshRenderer>();
	}
	public Vector2Int Position {
		set {
			transform.position = value.ToVector3Int();
		}
	}
	public Color PlayerColor {
		set {
			materialPropertyBlock.SetColor("_Color", value);
			foreach (var renderer in renderers)
				renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}