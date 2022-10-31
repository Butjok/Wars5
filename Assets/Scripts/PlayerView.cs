using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerView : MonoBehaviour {

	public static PlayerView DefaultPrefab => nameof(PlayerView).LoadAs<PlayerView>();
	
	public static List<PlayerView> views = new();
	public static bool globalVisibility = true;
	
	public static int playerColorId = Shader.PropertyToID("_Color");

	public static Func<int>[] layers = {
		() => Layers.Player0,
		() => Layers.Player1,
		() => Layers.Player2,
		() => Layers.Player3,
	};

	public Player player;
	public Renderer[] renderers;
	public MaterialPropertyBlock propertyBlock;
	public Camera camera;
	public bool visible;
	
	public void Initialize(Player player) {

		this.player = player;

		var usedLayers = player.game.players.Select(p => p.view.gameObject.layer);
		var availableLayers = layers.Select(f => f()).Except(usedLayers).ToArray();
		Assert.AreNotEqual(0, availableLayers.Length);
		var layer = availableLayers[0];
		gameObject.SetLayerRecursively(layer);
		camera.cullingMask |= 1 << layer;

		propertyBlock = new MaterialPropertyBlock();
		renderers = GetComponentsInChildren<Renderer>();

		propertyBlock.SetColor(playerColorId, player.color);
		foreach (var renderer in renderers)
			renderer.SetPropertyBlock(propertyBlock);
	}

	private void Update() {
		camera.enabled = visible && globalVisibility;
	}

	public void Awake() {
		views.Add(this);
	}
	public void OnDestroy() {
		views.Remove(this);
	}
}