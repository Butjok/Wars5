using System;
using UnityEngine;

public class BuildingView : MonoBehaviour {
	public BuildingView prefab;
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
		get => transform.position.ToVector2().RoundToInt();
		set => transform.position = value.Raycast();
	}
	public Vector2Int LookDirection {
		get => transform.forward.ToVector2().RoundToInt();
		set => transform.rotation = Quaternion.LookRotation(value.ToVector3Int(), Vector3.up);
	}
	public Color PlayerColor {
		set {
			materialPropertyBlock.SetColor("_PlayerColor", value);
			foreach (var renderer in renderers)
				renderer.SetPropertyBlock(materialPropertyBlock);
		}
	}
}