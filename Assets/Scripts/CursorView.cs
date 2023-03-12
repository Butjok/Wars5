using System;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
public class CursorView : MonoBehaviour {

	private static CursorView instance;
	public static bool TryFind(out CursorView result) {
		if (!instance)
			instance = FindObjectOfType<CursorView>();
		result = instance;
		return result;
	}
	public static CursorView TryFind() {
		TryFind(out var cursorView);
		return cursorView;
	}

	public MeshRenderer meshRenderer;
	[Command]
	public bool show = true;
	public Level level;
	public bool showGui = true;

	public bool showOnlyOnTiles = true;

	public Vector2Int LookDirection {
		get => transform.forward.ToVector2().RoundToInt();
		set => transform.rotation = Quaternion.LookRotation(value.ToVector3Int(), Vector3.up);
	}

	public void Reset() {
		meshRenderer = GetComponent<MeshRenderer>();
	}

	public void LateUpdate() {

		if (!level) {
			level = FindObjectOfType<Level>();
			Assert.IsTrue(level);
		}
		
		if (Mouse.TryGetPosition(out Vector2Int mousePosition) && (!showOnlyOnTiles || level.TryGetTile(mousePosition, out _))) {
			meshRenderer.enabled = show;
			transform.position = mousePosition.ToVector3Int();
		}
		else {
			meshRenderer.enabled = false;
		}
	}

	private void OnGUI() {
		if (showGui && Mouse.TryGetPosition(out Vector2Int position)) {
			GUI.skin = DefaultGuiSkin.TryGet;
			var content = new GUIContent(position.ToString());
			var size = GUI.skin.label.CalcSize(content);
			GUI.Label(new Rect(Screen.width-size.x, Screen.height-size.y, size.x, size.y), position.ToString());
		}
	}
}