using System;
using UnityEngine;

public class PlayerView : MonoBehaviour {

	public Lazy<Renderer[]> renderers;
	public Lazy<MaterialPropertyBlock> propertyBlock;
	public static int playerColorId = Shader.PropertyToID("_Color");
	public ChangeTracker<Color> playerColor;

	private void Awake() {
		renderers = new Lazy<Renderer[]>(GetComponentsInChildren<Renderer>);
		propertyBlock = new Lazy<MaterialPropertyBlock>(() => new MaterialPropertyBlock());
		playerColor = new ChangeTracker<Color>(_ => {
			propertyBlock.v.SetColor(playerColorId, playerColor.v);
			foreach (var renderer in renderers.v)
				renderer.SetPropertyBlock(propertyBlock.v);
		});
	}
}