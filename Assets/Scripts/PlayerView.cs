using System;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerView : MonoBehaviour {

    public static PlayerView DefaultPrefab => nameof(PlayerView).LoadAs<PlayerView>();

    public static List<PlayerView> views = new();
    [Command]
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

        var usedLayers = player.level.players.Select(p => p.view.gameObject.layer);
        var availableLayers = layers.Select(f => f()).Except(usedLayers).ToArray();
        Assert.AreNotEqual(0, availableLayers.Length);
        var layer = availableLayers[0];
        gameObject.SetLayerRecursively(layer);
        if (camera)
            camera.cullingMask |= 1 << layer;

        propertyBlock = new MaterialPropertyBlock();
        renderers = GetComponentsInChildren<Renderer>();

        propertyBlock.SetColor(playerColorId, player.Color);
        foreach (var renderer in renderers)
            renderer.SetPropertyBlock(propertyBlock);
    }

    private void Update() {
        if (camera)
            camera.enabled = visible && globalVisibility;
        //camera.enabled = false;
    }

    public void Awake() {
        views.Add(this);
    }
    public void OnDestroy() {
        views.Remove(this);
    }
}