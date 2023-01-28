using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public static class GameWriter {

	public static TextWriter Write(TextWriter tw, Main main) {

		WriteLine(tw, SceneManager.GetActiveScene().name, "game.load-scene");
		WriteLine(tw, $"{main.missionName} MissionName type enum", "game.set-mission-name");
		WriteLine(tw, main.turn, "game.set-turn");

		WriteLine(tw, "\n\n");

		foreach (var player in main.players) {

			WriteComment(tw);
			WriteComment(tw, $"Player {main.players.IndexOf(player)} {player}");
			WriteComment(tw);

			WriteLine(tw);

			AddPlayer(tw, player);
			WriteLine(tw);

			foreach (var building in player.main.FindBuildingsOf(player)) {
				WriteLine(tw, "dup");
				AddBuilding(tw, building);
				WriteLine(tw, "pop");
				WriteLine(tw);
			}

			WriteLine(tw);

			foreach (var unit in player.main.FindUnitsOf(player)) {
				WriteLine(tw, "dup");
				AddUnit(tw, unit);
				WriteLine(tw, "pop");
				WriteLine(tw);
			}

			WriteLine(tw, "pop");
			WriteLine(tw, "\n");
		}

		var tiles = main.tiles
			.Union(main.bridges.SelectMany(bridge => bridge.tiles));

		foreach (var (position, tileType) in tiles.OrderBy(kv=>kv.Value).ThenBy(kv => kv.Key.x).ThenBy(kv => kv.Key.y)) {
			if (TileType.Buildings.HasFlag(tileType))
				continue;
			//WriteLine(tw, $"{position.x} {position.y} int2", "tile.set-position");
			//WriteLine(tw, $"{tileType} TileType type enum", "tile.set-type");
			WriteLine(tw, $"{tileType} TileType type enum {position.x} {position.y} int2", "tile.add");
		}
		WriteLine(tw);

		foreach (var building in main.buildings.Values.Where(building => building.Player== null)) {
			WriteLine(tw, "null");
			AddBuilding(tw, building);
			WriteLine(tw, "pop");
			WriteLine(tw);
		}
		WriteLine(tw);

		foreach (var (trigger, positions) in main.triggers) {
			WriteLine(tw, $"{trigger.ToString().Replace(" ", "")} TriggerName type enum", "trigger.select");
			foreach (var position in positions)
				WriteLine(tw, $"{position.x} {position.y} int2", "trigger.add-position");
			WriteLine(tw);
		}

		foreach (var bridge in main.bridges) {
			AddBridge(tw, bridge);
			WriteLine(tw, "pop");
			WriteLine(tw);
		}
		WriteLine(tw);

		if (CameraRig.TryFind(out var cameraRig))
			WriteCameraRig(tw,cameraRig);

		return tw;
	}

	public static TextWriter WriteCameraRig(TextWriter tw,  CameraRig cameraRig) {

		var position = cameraRig.transform.position;
		WriteLine(tw, $"{position.x} {position.y} {position.z} float3", "camera-rig.set-position");
		WriteLine(tw, cameraRig.transform.rotation.eulerAngles.y, "camera-rig.set-rotation");
		WriteLine(tw, cameraRig.distance, "camera-rig.set-distance");
		WriteLine(tw, cameraRig.pitchAngle, "camera-rig.set-pitch-angle");
		
		return tw;
	}

	public static TextWriter AddBridge(TextWriter tw, Bridge bridge) {

		var gameObject = bridge.view.gameObject;
		Assert.AreNotEqual("Untagged", gameObject.tag);
		Assert.IsNotNull(gameObject.tag);
		WriteLine(tw, $"{gameObject.tag} find-with-tag", "");
		WriteLine(tw, "BridgeView type get-component", "bridge.set-view");
		WriteLine(tw, bridge.Hp, "bridge.set-hp");

		foreach (var position in bridge.tiles.Keys)
			WriteLine(tw, $"{position.x} {position.y} int2", "bridge.add-position");

		WriteLine(tw, "bridge.add");

		return tw;
	}

	public static TextWriter WriteComment(TextWriter tw, string text = "") {
		text = text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
		tw.WriteLine($"#{text}");
		return tw;
	}

	public static TextWriter AddBuilding(TextWriter tw, Building building) {
		WriteLine(tw, $"{building.type} TileType type enum", "building.set-type");
		WriteLine(tw, $"{building.position.x} {building.position.y} int2", "building.set-position");
		WriteLine(tw, building.Cp, "building.set-cp");
		WriteLine(tw, $"{building.view.LookDirection.x} {building.view.LookDirection.y} int2", "building.set-look-direction");
		if (building.type == TileType.MissileSilo) {
			WriteLine(tw, building.missileSiloLastLaunchTurn, "building.missile-silo.set-last-launch-turn");
			WriteLine(tw, building.missileSiloLaunchCooldown, "building.missile-silo.set-launch-cooldown");
		}
		WriteLine(tw, "building.add");
		return tw;
	}

	public static TextWriter AddPlayer(TextWriter tw, Player player) {

		WriteLine(tw, $"{player.Color.r} {player.Color.g} {player.Color.b}", "player.set-color");
		WriteLine(tw, $"{player.team} Team type enum", "player.set-team");
		WriteLine(tw, player.Credits, "player.set-credits");
		WriteLine(tw, player.AbilityMeter, "player.set-power-meter");
		WriteLine(tw, $"{player.unitLookDirection.x} {player.unitLookDirection.y} int2", "player.set-unit-look-direction");
		var index = player.main.players.IndexOf(player);
		WriteLine(tw, index,"player.on-additive-load-get-by-index");
		if (player.co)
			WriteLine(tw, player.co.name, "player.set-co");
		if (player.main.localPlayer == player)
			WriteLine(tw, "player.mark-as-local");
		if (player.abilityActivationTurn != null)
			WriteLine(tw, player.abilityActivationTurn, "player.set-ability-activation-turn");
		WriteLine(tw, "player.add");

		return tw;
	}

	public static TextWriter SelectPlayer(TextWriter tw, Player player) {
		var index = player.main.players.IndexOf(player);
		Assert.AreNotEqual(-1, index);
		WriteLine(tw, index, "player.select-by-index");
		return tw;
	}

	public static TextWriter AddUnit(TextWriter tw, Unit unit) {

		WriteLine(tw, $"{unit.type} UnitType type enum", "unit.set-type");
		WriteLine(tw, unit.Moved ? "true" : "false", "unit.set-moved");
		if (unit.Position is { } position)
			WriteLine(tw, $"{position.x} {position.y} int2", "unit.set-position");
		if (unit.view) {
			WriteLine(tw, $"{unit.view.LookDirection.x} {unit.view.LookDirection.y} int2", "unit.set-look-direction");
			if (unit.view.prefab)
				WriteLine(tw, $"{unit.view.prefab.name} UnitView type load-resource", "unit.set-view-prefab");
		}
		WriteLine(tw, "unit.add");

		if (unit.Cargo.Count != 0) {
			WriteLine(tw);
			foreach (var cargo in unit.Cargo) {
				WriteLine(tw, "dup"); // duplicate unit as a carrier
				WriteLine(tw, "dup"); // duplicate unit to get its player
				WriteLine(tw, "unit.get-player");
				AddUnit(tw, cargo);
				WriteLine(tw, "unit.put-into");
				WriteLine(tw);
			}
		}

		return tw;
	}

	private static TextWriter WriteLine(TextWriter tw, object left = null, object right = null) {
		if (left == null && right == null) {
			tw.WriteLine();
			return tw;
		}
		if (right == null) {
			right = left;
			left = "";
		}
		tw.WriteLine($"{left,-64} {right}");
		return tw;
	}
}