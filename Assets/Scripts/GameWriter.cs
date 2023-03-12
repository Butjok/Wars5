using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public static class GameWriter {

	static GameWriter() {
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
	}
	
	public static TextWriter Write(TextWriter tw, Level level, Predicate<Vector2Int> filter=null) {

		WriteLine(tw, SceneManager.GetActiveScene().name, "game.load-scene");
		WriteLine(tw, $"{level.missionName} MissionName type enum", "game.set-mission-name");
		WriteLine(tw, level.turn, "game.set-turn");

		WriteLine(tw, "\n\n");

		foreach (var player in level.players) {

			WriteComment(tw);
			WriteComment(tw, $"Player {level.players.IndexOf(player)} {player}");
			WriteComment(tw);

			WriteLine(tw);

			WritePlayer(tw, player);
			WriteLine(tw);

			foreach (var building in player.level.FindBuildingsOf(player)) {
				WriteLine(tw, "dup");
				WriteBuilding(tw, building);
				WriteLine(tw, "pop");
				WriteLine(tw);
			}

			WriteLine(tw);

			foreach (var unit in player.level.FindUnitsOf(player)) {
				WriteLine(tw, "dup");
				WriteUnit(tw, unit);
				WriteLine(tw, "pop");
				WriteLine(tw);
			}

			WriteLine(tw, "pop");
			WriteLine(tw, "\n");
		}

		var tiles = level.tiles
			.Union(level.bridges.SelectMany(bridge => bridge.tiles));

		foreach (var (position, tileType) in tiles.OrderBy(kv=>kv.Value).ThenBy(kv => kv.Key.x).ThenBy(kv => kv.Key.y)) {
			if (TileType.Buildings.HasFlag(tileType))
				continue;
			//WriteLine(tw, $"{position.x} {position.y} int2", "tile.set-position");
			//WriteLine(tw, $"{tileType} TileType type enum", "tile.set-type");
			WriteLine(tw, $"{tileType} TileType type enum {position.x} {position.y} int2", "tile.add");
		}
		WriteLine(tw);

		foreach (var building in level.buildings.Values.Where(building => building.Player== null)) {
			WriteLine(tw, "null");
			WriteBuilding(tw, building);
			WriteLine(tw, "pop");
			WriteLine(tw);
		}
		WriteLine(tw);

		foreach (var (trigger, positions) in level.triggers) {
			WriteLine(tw, $"{trigger.ToString().Replace(" ", "")} TriggerName type enum", "trigger.select");
			foreach (var position in positions)
				WriteLine(tw, $"{position.x} {position.y} int2", "trigger.add-position");
			WriteLine(tw);
		}

		foreach (var bridge in level.bridges) {
			WriteBridge(tw, bridge);
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
		WriteLine(tw, cameraRig.Fov, "camera-rig.set-fov");
		
		return tw;
	}

	public static TextWriter WriteBridge(TextWriter tw, Bridge bridge) {

		var gameObject = bridge.view.gameObject;
		Assert.AreNotEqual("Untagged", gameObject.tag);
		Assert.IsNotNull(gameObject.tag);
		WriteLine(tw, "BridgeView type true find-single-object-of-type", "bridge.set-view");
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

	public static TextWriter WriteBuilding(TextWriter tw, Building building) {
		WriteLine(tw, $"{building.type} TileType type enum", "building.set-type");
		WriteLine(tw, $"{building.position.x} {building.position.y} int2", "building.set-position");
		WriteLine(tw, building.Cp, "building.set-cp");
		WriteLine(tw, $"{building.view.LookDirection.x} {building.view.LookDirection.y} int2", "building.set-look-direction");
		if (building.type == TileType.MissileSilo) {
			WriteLine(tw, building.missileSiloLastLaunchTurn, "building.missile-silo.set-last-launch-turn");
			WriteLine(tw, building.missileSiloLaunchCooldown, "building.missile-silo.set-launch-cooldown");
			WriteLine(tw, building.missileSiloAmmo, "building.missile-silo.set-ammo");
			WriteLine(tw, $"{building.missileSiloRange[0]} {building.missileSiloRange[1]} int2", "building.missile-silo.set-range");
			WriteLine(tw, $"{building.missileBlastRange[0]} {building.missileBlastRange[1]} int2", "building.missile-silo.missile.set-blast-range");
			WriteLine(tw, building.missileUnitDamage, "building.missile-silo.missile.set-unit-damage");
			WriteLine(tw, building.missileBridgeDamage, "building.missile-silo.missile.set-bridge-damage");
		}
		WriteLine(tw, "building.add");
		return tw;
	}

	public static TextWriter WritePlayer(TextWriter tw, Player player) {

		WriteLine(tw, $"{player.ColorName} ColorName type enum", "player.set-color-name");
		WriteLine(tw, $"{player.team} Team type enum", "player.set-team");
		WriteLine(tw, $"{player.coName} PersonName type enum", "player.set-co-name");
		WriteLine(tw, $"{player.uiPosition.x} {player.uiPosition.y} int2", "player.set-ui-position");
		WriteLine(tw, player.Credits, "player.set-credits");
		WriteLine(tw, player.AbilityMeter, "player.set-power-meter");
		WriteLine(tw, $"{player.unitLookDirection.x} {player.unitLookDirection.y} int2", "player.set-unit-look-direction");
		WriteLine(tw, player.side, "player.set-side");
		if (player.level.localPlayer == player)
			WriteLine(tw, "player.mark-as-local");
		if (player.abilityActivationTurn != null)
			WriteLine(tw, player.abilityActivationTurn, "player.set-ability-activation-turn");
		WriteLine(tw, "player.add");

		return tw;
	}

	public static TextWriter WriteUnit(TextWriter tw, Unit unit) {

		WriteLine(tw, $"{unit.type} UnitType type enum", "unit.set-type");
		WriteLine(tw, unit.Moved ? "true" : "false", "unit.set-moved");
		WriteLine(tw, unit.Hp, "unit.set-hp");
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
				WriteUnit(tw, cargo);
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