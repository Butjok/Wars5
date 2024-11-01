// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Linq;
// using System.Text;
// using Butjok.CommandLine;
// using Drawing;
// using UnityEngine;
// using UnityEngine.Assertions;
// using Debug = UnityEngine.Debug;
// using Random = UnityEngine.Random;
// using static Rules;
//
// public class LevelEditor {
//
//
//     
//
//     public GUISkin guiSkin;
//
//     public Color abilityStripeFullColor = Color.yellow;
//     public Color abilityStripeFilledColor = Color.white;
//     public Color abilityStripeUnfilledColor = Color.gray;
//     public Color abilityStripeActiveColor = Color.yellow;
//     public string abilityStripeFilledSymbol = "*";
//     public string abilityStripeUnfilledSymbol = "*";
//     public string abilityStripeActiveText = " [ACTIVE]";
//
//     public string stackLeakFormat = "<b><color=yellow>{0}</color></b>";
//
//     private void Start() {
//         if (StateMachine.Instance.IsEmpty) {
//             StateMachine.Instance.Push("LevelEditor", Run());
//             if (TurnButton.TryGet(out var turnButton))
//                 turnButton.Visible = false;
//         }
//     }
//
//     protected void OnGUI() {
//
//         GUI.skin = guiSkin;
//
//         
//
//         var labelHeight = GUI.skin.label.CalcHeight(GUIContent.none, 1);
//         GUILayout.Space(labelHeight);
//
//         var topLine = "";
//         topLine += "stack: ";
//         topLine += stack.Count < 10 ? stack.Count.ToString() : string.Format(stackLeakFormat, stack.Count);
//         topLine += "";
//
//         GUILayout.Label(topLine);
//
//         if (showPlayInfo) {
//
//             var filled = CurrentPlayer.AbilityMeter;
//             var max = MaxAbilityMeter(CurrentPlayer);
//
//             var abilityStripeBuilder = new StringBuilder();
//             if (AbilityInUse(CurrentPlayer)) {
//                 abilityStripeBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(abilityStripeActiveColor)}>");
//                 for (var i = 0; i < max; i++)
//                     abilityStripeBuilder.Append(abilityStripeFilledSymbol);
//                 abilityStripeBuilder.Append("</color>");
//                 abilityStripeBuilder.Append(abilityStripeActiveText);
//             }
//             else {
//                 if (filled > 0) {
//                     abilityStripeBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(abilityStripeFilledColor)}>");
//                     for (var i = 0; i < filled; i++)
//                         abilityStripeBuilder.Append(abilityStripeFilledSymbol);
//                     abilityStripeBuilder.Append("</color>");
//                 }
//                 if (max - filled > 0) {
//                     abilityStripeBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(abilityStripeUnfilledColor)}>");
//                     for (var i = 0; i < max - filled; i++)
//                         abilityStripeBuilder.Append(abilityStripeUnfilledSymbol);
//                     abilityStripeBuilder.Append("</color>");
//                 }
//             }
//
//             var credits = CurrentPlayer.Credits;
//             var playerHtmlColor = ColorUtility.ToHtmlStringRGB(CurrentPlayer.Color);
//             GUILayout.Label($"{turn} · <color=#{playerHtmlColor}>{CurrentPlayer.coName}</color> · {credits} · {abilityStripeBuilder}");
//         }
//         else
//             foreach (var (name, value) in screenText.Where(kv => screenTextFilters.Any(filter => filter(kv.Key))).OrderBy(kv => kv.Key))
//
//
//                 if (inspectedUnit != null) {
//
//                     CameraRig.TryFind(out var cameraRig);
//
//                     GUILayout.Space(labelHeight);
//
//                     GUILayout.Label($"type:\t{inspectedUnit.type}");
//                     GUILayout.Label($"player:\t{inspectedUnit.Player}");
//                     GUILayout.Label($"position:\t{inspectedUnit.Position}");
//                     GUILayout.Label($"moved:\t{inspectedUnit.Moved}");
//                     GUILayout.Label($"hp:\t{inspectedUnit.Hp} / {MaxHp(inspectedUnit)}");
//                     GUILayout.Label($"moveDistance:\t{MoveCapacity(inspectedUnit)}");
//
//                     GUILayout.Label($"fuel:\t{inspectedUnit.Fuel} / {MaxFuel(inspectedUnit)}");
//                     var weaponNames = GetWeaponNames(inspectedUnit).ToArray();
//                     if (weaponNames.Length > 0) {
//                         GUILayout.Label($"ammo:");
//                         foreach (var weaponName in weaponNames) {
//                             var amount = inspectedUnit.GetAmmo(weaponName);
//                             var text = amount > 10000 && MaxAmmo(inspectedUnit, weaponName) == int.MaxValue ? "∞" : $"{amount} / {MaxAmmo(inspectedUnit, weaponName)}";
//                             GUILayout.Label($"- {weaponName}:\t{text}");
//                         }
//                     }
//
//                     if (inspectedUnit.Carrier is { Disposed: false }) {
//                         GUILayout.BeginHorizontal();
//                         GUILayout.Label($"carrier:\t{inspectedUnit.Carrier}");
//                         if (GUILayout.Button(jumpButtonText)) {
//                             if (cameraRig)
//                                 cameraRig.Jump(inspectedUnit.Carrier.view.transform.position);
//                         }
//                         GUILayout.EndHorizontal();
//                     }
//                     if (inspectedUnit.Cargo.Count > 0) {
//                         GUILayout.Label($"cargo ({inspectedUnit.Cargo.Sum(c => Weight(c))} / {CarryCapacity(inspectedUnit)}):");
//                         foreach (var cargo in inspectedUnit.Cargo)
//                             GUILayout.Label($"- {cargo} ({Weight(cargo)})");
//                     }
//
//                     GUILayout.BeginHorizontal();
//                     GUILayout.Label($"view:\t{inspectedUnit.view}");
//                     if (GUILayout.Button(jumpButtonText)) {
//                         if (cameraRig)
//                             cameraRig.Jump(inspectedUnit.view.transform.position);
//                     }
//                     GUILayout.EndHorizontal();
//                 }
//     }
//
//     public string jumpButtonText = "➲";
//
//     public float inspectionCircleRadius = .4f;
//     public float inspectionCircleLineWidth = 2;
//     [ColorUsage(false, true)] public Color inspectionCircleColor = Color.yellow;
//     public Color inspectionTextColor = Color.yellow;
//     public Color attackPositionColor = Color.red;
//
//     public Color fullAbilityStripeColor = Color.yellow;
//
//     [Command]
//     public bool TrySetPlayerAbilityMeter(int index, int value) {
//         if (index < 0 || index >= players.Count)
//             return false;
//         players[index].SetAbilityMeter(value, true, true);
//         return true;
//     }
//     [Command]
//     public bool TrySetPlayerAbilityActivationTurn(int index, int? value) {
//         if (index < 0 || index >= players.Count)
//             return false;
//         players[index].abilityActivationTurn = value;
//         return true;
//     }
//
//     
//
//     
//
//     protected override void OnApplicationQuit() {
//
//         base.OnApplicationQuit();
//
//         StateMachine.Instance.Pop(all: true);
//
//         
//
//         Clear();
//         if (Player.undisposed.Count > 0)
//             Debug.LogError($"undisposed players: {Player.undisposed.Count}");
//         if (Building.undisposed.Count > 0)
//             Debug.LogError($"undisposed buildings: {Building.undisposed.Count}");
//         if (Unit.undisposed.Count > 0)
//             Debug.LogError($"undisposed units: {Unit.undisposed.Count}");
//         if (UnitAction.undisposed.Count > 0)
//             Debug.LogError($"undisposed unit actions: {UnitAction.undisposed.Count}");
//     }
//
//     protected override void Update() {
//         base.Update();
//
//         if (showBridges)
//             foreach (var bridge in bridges) {
//                 var index = bridges.IndexOf(bridge);
//                 if (bridge.tiles.Count > 0) {
//
//                     var center = Vector2.zero;
//                     var count = 0;
//                     foreach (var position in bridge.tiles.Keys) {
//                         center += position;
//                         count++;
//
//                         Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, Color.white);
//                     }
//
//                     center /= count;
//                     Draw.ingame.Label2D(center.ToVector3(), $"Bridge{index}: {bridge.Hp}", 14, LabelAlignment.Center, Color.black);
//                 }
//             }
//
//         if (Input.GetKeyDown(KeyCode.Return) && Mouse.TryGetPosition(out Vector2Int mousePosition))
//             TryGetUnit(mousePosition, out inspectedUnit);
//
//
//         if (inspectedUnit != null) {
//             if (inspectedUnit.Disposed)
//                 inspectedUnit = null;
//             else if (inspectedUnit.Position is { } position) {
//                 Vector3 position3d = position.ToVector3Int();
//
//                 using (Draw.ingame.WithLineWidth(inspectionCircleLineWidth)) {
//                     Draw.ingame.CircleXZ(position3d, inspectionCircleRadius, inspectionCircleColor);
//                 }
//
//                 var attackPositions = Enumerable.Empty<Vector2Int>();
//                 if (TryGetAttackRange(inspectedUnit, out var attackRange)) {
//                     if (IsArtillery(inspectedUnit))
//                         attackPositions = PositionsInRange(position, attackRange);
//                     else {
//                         //traverser.Traverse(tiles.Keys, position, Rules.MoveCost(), Rules.MoveDistance(inspectedUnit));
//                     }
//                 }
//
//                 foreach (var attackPosition in attackPositions)
//                     Draw.ingame.SolidPlane((Vector3)attackPosition.ToVector3Int() + Vector3.up * offset, Vector3.up, Vector2.one, attackPositionColor);
//             }
//         }
//
//         if (debugVisionCapacity is { } visionCapacity) {
//             if (Mouse.TryGetPosition(out mousePosition) && TryGetTile(mousePosition, out var tileType)) {
//                 foreach (var position in FogOfWar.CalculateVision(tiles, mousePosition, visionCapacity + (!debugVisionAirborne && tileType == TileType.Mountain ? FogOfWar.mountainCapacityBonus : 0), debugVisionAirborne))
//                     Draw.ingame.SolidPlane((Vector3)position.ToVector3Int(), Vector3.up, Vector2.one, debugVisionColor);
//             }
//         }
//     }
//
//     public const string prefix = "level-editor.";
//
//     public const string selectTilesMode = prefix + "select-tiles-mode";
//     public const string selectUnitsMode = prefix + "select-units-mode";
//     public const string selectTriggersMode = prefix + "select-triggers-mode";
//     public const string selectBridgesMode = prefix + "select-bridges-mode";
//
//     public const string cycleTileType = prefix + "cycle-tile-type";
//     public const string placeTile = prefix + "place-tile";
//     public const string removeTile = prefix + "remove-tile";
//
//     public const string placeTrigger = prefix + "place-trigger";
//     public const string removeTrigger = prefix + "remove-trigger";
//
//     public const string cycleUnitType = prefix + "cycle-unit";
//     public const string placeUnit = prefix + "place-unit";
//     public const string removeUnit = prefix + "remove-unit";
//     public const string inspectUnit = prefix + "inspect-unit";
//
//     public const string pickTile = prefix + "pick-tile";
//     public const string pickTrigger = prefix + "pick-trigger";
//     public const string pickUnit = prefix + "pick-unit";
//
//     public const string cyclePlayer = prefix + "cycle-players";
//     public const string play = prefix + "play";
//
//     public const string cycleTrigger = prefix + "cycle-trigger";
//
//     public const string mode = nameof(mode);
//     public const string autosave = prefix + "autosave";
//
//     [Command]
//     public int? debugVisionCapacity;
//     [Command]
//     public bool debugVisionAirborne;
//     [Command]
//     public Color debugVisionColor = new Color(0f, 0.89f, 1f, 0.51f);
//
//     public Stack<(Action perform, Action revert)> undos = new();
//     public Stack<(Action perform, Action revert)> redos = new();
//
//     public TriggerName[] triggerNames = { TriggerName.A, TriggerName.B, TriggerName.C, TriggerName.D, TriggerName.E, TriggerName.F };
//     public TriggerName triggerName = TriggerName.A;
//
//     public bool showBridges;
//     [Command]
//     public void ToggleBridges() {
//         showBridges = !showBridges;
//     }
//
//     public bool TryFindBridge(Vector2Int position, out Bridge bridge) {
//         bridge = bridges.SingleOrDefault(b => b.tiles.Keys.Contains(position));
//         return bridge != null;
//     }
//
//     [Command]
//     public bool TrySetBridgeHp(int hp) {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryFindBridge(position, out var bridge))
//             return false;
//         bridge.SetHp(hp);
//         return true;
//     }
//     [Command]
//     public bool TryRemoveBridge() {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryFindBridge(position, out var bridge))
//             return false;
//         bridges.Remove(bridge);
//         return true;
//     }
//
//     [Command]
//     public void RemoveTrigger(TriggerName triggerName) {
//         Assert.IsTrue(triggers.ContainsKey(triggerName));
//         triggers[triggerName].Clear();
//     }
//
//     [Command]
//     public bool TrySetUnitHp(int hp) {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetUnit(position, out var unit))
//             return false;
//         unit.SetHp(hp);
//         return true;
//     }
//
//     [Command]
//     public bool TrySetUnitFuel(int fuel) {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetUnit(position, out var unit))
//             return false;
//         unit.Fuel = fuel;
//         return true;
//     }
//
//     [Command]
//     public void ResetToDefaultLevel() {
//
//         Clear();
//
//         var red = new Player(this, ColorName.Red, Team.Alpha, credits: 16000, unitLookDirection: Vector2Int.right);
//         var blue = new Player(this, ColorName.Blue, Team.Bravo, credits: 16000, unitLookDirection: Vector2Int.left);
//         localPlayer = red;
//         player = red;
//
//         RebuildTilemapMesh();
//     }
//
//     public IEnumerator<StateChange> Run() {
//
//         if (!TryGetLatestSaveFilePath("autosave", out var path))
//             ResetToDefaultLevel();
//         else
//             Load("autosave");
//
//         screenText["tile-type"] = () => tileType;
//         screenText["player"] = () => players.IndexOf(player);
//         screenText["unit-type"] = () => unitType;
//         screenText["trigger-name"] = () => triggerName;
//
//         yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
//     }
//
//     public TileType tileType = TileType.Plain;
//     public TileType[] tileTypes = { TileType.Plain, TileType.Road, TileType.Forest, TileType.Mountain, TileType.River, TileType.Sea, TileType.City, TileType.Hq, TileType.Factory, TileType.Airport, TileType.Shipyard, TileType.MissileSilo };
//
//     public Player player;
//
//     public IEnumerator<StateChange> TilesMode() {
//
//         ClearScreenTextFilters();
//         AddScreenTextPrefixFilter("tiles-mode.");
//         AddScreenTextFilter("tile-type", "player", "look-direction");
//
//         if (CursorView.TryFind(out var cursorView))
//             cursorView.show = true;
//
//         while (true) {
//             yield return StateChange.none;
//
//             if (Input.GetKeyDown(KeyCode.F8))
//                 commands.Enqueue(selectUnitsMode);
//             else if (Input.GetKeyDown(KeyCode.Tab)) {
//                 stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
//                 commands.Enqueue(cycleTileType);
//             }
//             else if (Input.GetKeyDown(KeyCode.F2)) {
//                 stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
//                 commands.Enqueue(cyclePlayer);
//             }
//             else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int addPosition)) {
//                 stack.Push(player);
//                 stack.Push(tileType);
//                 stack.Push(addPosition);
//                 commands.Enqueue(placeTile);
//             }
//             else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
//                 stack.Push(removePosition);
//                 commands.Enqueue(removeTile);
//             }
//             else if (Input.GetKeyDown(KeyCode.F5))
//                 commands.Enqueue(play);
//
//             // else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
//             //     stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
//             //     commands.Enqueue(cycleLookDirection);
//             // }
//             else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
//                 stack.Push(pickPosition);
//                 commands.Enqueue(pickTile);
//             }
//
//             while (commands.TryDequeue(out var command))
//                 foreach (var token in Tokenizer.Tokenize(command))
//                     switch (token) {
//
//                         case selectUnitsMode:
//                             yield return StateChange.ReplaceWith(nameof(UnitsMode), UnitsMode());
//                             break;
//
//                         case cycleTileType:
//                             tileType = tileType.Cycle(tileTypes, stack.Pop<int>());
//                             break;
//
//                         case cyclePlayer: {
//                             player = player.Cycle(players.Concat(new[] { (Player)null }), stack.Pop<int>());
//                             break;
//                         }
//
//                         case placeTile: {
//
//                             var position = stack.Pop<Vector2Int>();
//                             var tileType = stack.Pop<TileType>();
//                             var player = stack.Pop<Player>();
//
//                             if (tiles.ContainsKey(position))
//                                 TryRemoveTile(position, false);
//
//                             tiles.Add(position, tileType);
//                             if (TileType.Buildings.HasFlag(tileType))
//                                 new Building(this, position, tileType, player, viewPrefab: BuildingView.GetPrefab(tileType), lookDirection: player?.unitLookDirection ?? Vector2Int.up);
//
//                             RebuildTilemapMesh();
//
//                             break;
//                         }
//
//                         case removeTile:
//                             TryRemoveTile(stack.Pop<Vector2Int>(), true);
//                             RebuildTilemapMesh();
//                             break;
//
//                         case play:
//                             yield return StateChange.Push(nameof(Play), Play());
//                             yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
//                             break;
//
//                         case pickTile: {
//                             var position = stack.Pop<Vector2Int>();
//                             if (tiles.TryGetValue(position, out var pickedTileType))
//                                 tileType = pickedTileType;
//                             if (buildings.TryGetValue(position, out var building)) {
//                                 player = building.Player;
//                             }
//                             break;
//                         }
//
//                         default:
//                             stack.ExecuteToken(token);
//                             break;
//                     }
//         }
//     }
//
//     private List<Vector3> vertices = new List<Vector3>();
//     private List<int> triangles = new List<int>();
//     private List<Color> colors = new List<Color>();
//
//     private Mesh quadMesh;
//     private Vector3[] quadVertices;
//     private int[] quadTriangles;
//
//     private Mesh forestMesh;
//     private Vector3[] forestVertices;
//     private int[] forestTriangles;
//
//     private Mesh mountainMesh;
//     private Vector3[] mountainVertices;
//     private int[] mountainTriangles;
//
//     [Command]
//     public string InspectPlayer(int index) {
//         if (index < 0 || index > players.Count)
//             return null;
//
//         using var sw = new StringWriter();
//         LevelWriter.WritePlayer(sw, players[index]);
//         return sw.ToString();
//     }
//
//     [Command]
//     public string InspectUnit() {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetUnit(position, out var unit))
//             return null;
//
//         using var sw = new StringWriter();
//         LevelWriter.WriteUnit(sw, unit);
//         return sw.ToString();
//     }
//
//     [Command]
//     public string InspectBuilding() {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetBuilding(position, out var building))
//             return null;
//
//         using var sw = new StringWriter();
//         LevelWriter.WriteBuilding(sw, building);
//         return sw.ToString();
//     }
//
//     [Command]
//     public string InspectBridge() {
//         if (!Mouse.TryGetPosition(out Vector2Int position) || !TryGetBridge(position, out var bridge))
//             return null;
//
//         using var sw = new StringWriter();
//         LevelWriter.WriteBridge(sw, bridge);
//         return sw.ToString();
//     }
//
//     public void RebuildTilemapMesh() {
//
//         vertices.Clear();
//         triangles.Clear();
//         colors.Clear();
//
//         void LoadMesh(string name, out Mesh mesh, out Vector3[] vertices, out int[] triangles) {
//             mesh = name.LoadAs<Mesh>();
//             vertices = mesh.vertices;
//             triangles = mesh.triangles;
//         }
//
//         if (!quadMesh)
//             LoadMesh("quad", out quadMesh, out quadVertices, out quadTriangles);
//         if (!forestMesh)
//             LoadMesh("forest-placeholder", out forestMesh, out forestVertices, out forestTriangles);
//         if (!mountainMesh)
//             LoadMesh("mountain-placeholder", out mountainMesh, out mountainVertices, out mountainTriangles);
//
//         foreach (var position in tiles.Keys) {
//
//             var tileType = tiles[position];
//             var color = buildings.TryGetValue(position, out var building) && building.Player != null
//                 ? building.Player.Color
//                 : Color.white;
//             color.a = (int)tiles[position];
//
//             var source = tileType switch {
//                 TileType.Forest => (forestVertices, forestTriangles, Enumerable.Repeat(color, forestVertices.Length)),
//                 TileType.Mountain => (mountainVertices, mountainTriangles, Enumerable.Repeat(color, mountainVertices.Length)),
//                 _ => (quadVertices, quadTriangles, Enumerable.Repeat(color, quadVertices.Length))
//             };
//
//             Random.InitState(position.GetHashCode());
//             MeshUtils.AppendMesh(
//                 (vertices, triangles, colors),
//                 source,
//                 Matrix4x4.TRS(position.ToVector3Int(), Quaternion.Euler(0, Random.Range(0, 4) * 90, 0), Vector3.one));
//         }
//
//         var mesh = new Mesh {
//             vertices = vertices.ToArray(),
//             triangles = triangles.ToArray(),
//             colors = colors.ToArray()
//         };
//
//         mesh.Optimize();
//
//         mesh.RecalculateBounds();
//         mesh.RecalculateNormals();
//         mesh.RecalculateTangents();
//
//         if (tileMeshFilter.sharedMesh)
//             Destroy(tileMeshFilter.sharedMesh);
//         if (tileMeshCollider.sharedMesh)
//             Destroy(tileMeshCollider.sharedMesh);
//
//         tileMeshFilter.sharedMesh = mesh;
//         tileMeshCollider.sharedMesh = mesh;
//     }
//
//     
//
//     
//
//     public IEnumerator<StateChange> UnitsMode() {
//
//         if (player == null) {
//             Assert.AreNotEqual(0, players.Count);
//             player = players[0];
//         }
//
//         ClearScreenTextFilters();
//         AddScreenTextPrefixFilter("units-mode.");
//         AddScreenTextFilter("unit-type", "look-direction", "player");
//
//         if (CursorView.TryFind(out var cursorView))
//             cursorView.show = true;
//
//         while (true) {
//             yield return StateChange.none;
//
//             if (Input.GetKeyDown(KeyCode.F8))
//                 commands.Enqueue(selectTriggersMode);
//
//             else if (Input.GetKeyDown(KeyCode.Tab)) {
//                 stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
//                 commands.Enqueue(cycleUnitType);
//             }
//             else if (Input.GetKeyDown(KeyCode.F2)) {
//                 stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
//                 commands.Enqueue(cyclePlayer);
//             }
//             else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int position) && tiles.ContainsKey(position)) {
//                 stack.Push(player);
//                 stack.Push(unitType);
//                 stack.Push(position);
//                 commands.Enqueue(placeUnit);
//             }
//             else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out position)) {
//                 stack.Push(position);
//                 commands.Enqueue(removeUnit);
//             }
//             // else if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown)) {
//             //     stack.Push(Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1);
//             //     commands.Enqueue(cycleLookDirection);
//             // }
//             else if (Input.GetKeyDown(KeyCode.F5))
//                 commands.Enqueue(play);
//
//             else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out position)) {
//                 stack.Push(position);
//                 commands.Enqueue(pickUnit);
//             }
//             /*else if (Input.GetKeyDown(KeyCode.Return) && Mouse.TryGetPosition(out position)) {
//                 stack.Push(position);
//                 commands.Enqueue(inspectUnit);
//             }*/
//
//             while (commands.TryDequeue(out var command))
//                 foreach (var token in Tokenizer.Tokenize(command))
//                     switch (token) {
//
//                         case selectTriggersMode:
//                             yield return StateChange.ReplaceWith(nameof(TriggersMode), TriggersMode());
//                             break;
//
//                         case cyclePlayer:
//                             player = player.Cycle(players, stack.Pop<int>());
//                             break;
//
//                         case cycleUnitType:
//                             unitType = unitType.Cycle(unitTypes, stack.Pop<int>());
//                             break;
//
//                         case play:
//                             yield return StateChange.Push(nameof(Play), Play());
//                             yield return StateChange.ReplaceWith(nameof(UnitsMode), UnitsMode());
//                             break;
//
//                         case placeUnit: {
//
//                             var position = stack.Pop<Vector2Int>();
//                             var unitType = stack.Pop<UnitType>();
//                             var player = stack.Pop<Player>();
//
//                             if (units.ContainsKey(position))
//                                 TryRemoveUnit(position);
//
//                             var viewPrefab = (unitType switch {
//                                 UnitType.Artillery => "WbHowitzerRigged",
//                                 UnitType.Apc => "WbApcRigged",
//                                 UnitType.Recon => "WbReconRigged",
//                                 UnitType.LightTank => "WbLightTankRigged",
//                                 UnitType.Rockets => "WbRocketsRigged",
//                                 UnitType.MediumTank => "WbMdTankRigged",
//                                 _ => "WbLightTankRigged"
//                             }).LoadAs<UnitView>();
//
//                             new Unit(player, unitType, position, player.unitLookDirection, viewPrefab: viewPrefab);
//                             break;
//                         }
//
//                         case removeUnit:
//                             TryRemoveUnit(stack.Pop<Vector2Int>());
//                             break;
//
//                         case pickUnit: {
//                             var position = stack.Pop<Vector2Int>();
//                             if (units.TryGetValue(position, out var unit)) {
//                                 unitType = unit.type;
//                                 player = unit.Player;
//                             }
//                             break;
//                         }
//
//                         case inspectUnit: {
//                             var position = stack.Pop<Vector2Int>();
//                             units.TryGetValue(position, out inspectedUnit);
//                             break;
//                         }
//
//                         default:
//                             stack.ExecuteToken(token);
//                             break;
//                     }
//         }
//     }
//
//     public Unit inspectedUnit;
//
//     public bool TryRemoveUnit(Vector2Int position) {
//         if (!units.TryGetValue(position, out var unit))
//             return false;
//         unit.Dispose();
//         return true;
//     }
//
//     public IEnumerator<StateChange> TriggersMode() {
//
//         ClearScreenTextFilters();
//         AddScreenTextPrefixFilter("triggers-mode.");
//         AddScreenTextFilter("trigger-name");
//
//         if (CursorView.TryFind(out var cursorView))
//             cursorView.show = true;
//
//         while (true) {
//             yield return StateChange.none;
//
//             if (Input.GetKeyDown(KeyCode.F8))
//                 commands.Enqueue(selectTilesMode);
//
//             else if (Input.GetKeyDown(KeyCode.Tab)) {
//                 stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
//                 commands.Enqueue(cycleTrigger);
//             }
//             else if (Input.GetMouseButton(Mouse.left) && Mouse.TryGetPosition(out Vector2Int addPosition)) {
//                 stack.Push(addPosition);
//                 commands.Enqueue(placeTrigger);
//             }
//             else if (Input.GetMouseButton(Mouse.right) && Mouse.TryGetPosition(out Vector2Int removePosition)) {
//                 stack.Push(removePosition);
//                 commands.Enqueue(removeTrigger);
//             }
//             else if (Input.GetKeyDown(KeyCode.F5))
//                 commands.Enqueue(play);
//
//             else if (Input.GetKeyDown(KeyCode.LeftAlt) && Mouse.TryGetPosition(out Vector2Int pickPosition)) {
//                 stack.Push(pickPosition);
//                 commands.Enqueue(pickTrigger);
//             }
//
//             while (commands.TryDequeue(out var command))
//                 foreach (var token in Tokenizer.Tokenize(command))
//                     switch (token) {
//
//                         case selectTilesMode:
//                             yield return StateChange.ReplaceWith(nameof(TilesMode), TilesMode());
//                             break;
//
//                         case cycleTrigger:
//                             triggerName = triggerName.Cycle(triggerNames, stack.Pop<int>());
//                             Assert.IsTrue(triggers.ContainsKey(triggerName));
//                             break;
//
//                         case placeTrigger: {
//                             var position = stack.Pop<Vector2Int>();
//                             triggers[triggerName].Add(position);
//                             break;
//                         }
//
//                         case removeTrigger: {
//                             var position = stack.Pop<Vector2Int>();
//                             foreach (var (_, set) in triggers)
//                                 set.Remove(position);
//                             break;
//                         }
//
//                         case play:
//                             yield return StateChange.Push(nameof(Play), Play());
//                             yield return StateChange.ReplaceWith(nameof(TriggersMode), TriggersMode());
//                             break;
//
//                         case pickTrigger: {
//                             var position = stack.Pop<Vector2Int>();
//                             var candidates = triggers.Where(kv => kv.Value.Contains(position)).Select(kv => kv.Key).ToArray();
//                             if (candidates.Length > 0)
//                                 triggerName = triggerName.Cycle(candidates, 1);
//                             break;
//                         }
//
//                         default:
//                             stack.ExecuteToken(token);
//                             break;
//                     }
//
//             var positions = triggers.SelectMany(t => t.Value).Distinct();
//             foreach (var position in positions) {
//
//                 var triggerNames = triggers.Keys.Where(t => triggers[t].Contains(position)).ToArray();
//                 var color = Color.black;
//                 foreach (var triggerName in triggerNames) {
//                     if (triggerColors.TryGetValue(triggerName, out var triggerColor))
//                         color += triggerColor;
//                 }
//
//                 Draw.ingame.SolidPlane((Vector3)position.ToVector3Int() + Vector3.up * offset, Vector3.up, Vector2.one, color);
//                 Draw.ingame.Label2D((Vector3)position.ToVector3Int(), string.Join(",", triggerNames), Color.white);
//             }
//         }
//     }
//
//     public float offset = .01f;
//     public TriggerNameColorDictionary triggerColors = new() {
//         [TriggerName.A] = Color.red,
//         [TriggerName.B] = Color.green,
//         [TriggerName.C] = Color.blue,
//         [TriggerName.D] = Color.cyan,
//         [TriggerName.E] = Color.yellow,
//         [TriggerName.F] = Color.magenta
//     };
//
//     public bool showPlayInfo;
//
//     public IEnumerator<StateChange> Play() {
//
//         //autoplay = true;
//
//         ClearScreenTextFilters();
//         AddScreenTextPrefixFilter("play-mode.");
//
//         if (CursorView.TryFind(out var cursorView))
//             cursorView.show = false;
//
//         using var tw = new StringWriter();
//         LevelWriter.WriteLevel(tw, this);
//         var save = tw.ToString();
//         // Debug.Log(save);
//         var playerIndex = players.IndexOf(player);
//         levelLogic = new LevelLogic();
//
//         UpdatePlayBorderStyle();
//         showPlayInfo = true;
//         showPlayBorder = true;
//
//         //if (aiPlayerCommander)
// //            aiPlayerCommander.StartPlaying();
//
//         if (TurnButton.TryGet(out var turnButton))
//             turnButton.Visible = true;
//
//         yield return StateChange.Push(new PlayerTurnState(this));
//
//         if (turnButton) {
//             turnButton.Day = null;
//             turnButton.Visible = false;
//         }
//
//         //      if (aiPlayerCommander)
//         //        aiPlayerCommander.StopPlaying();
//
//         showPlayInfo = false;
//         showPlayBorder = false;
//
//         LoadFromText(save);
//         player = playerIndex == -1 ? null : players[playerIndex];
//     }
//
//     
//
//     
//
//     [Command]
//     public void Clear() {
//
//         turn = 0;
//
//         foreach (var player in players.ToArray())
//             player.Dispose();
//         players.Clear();
//
//         localPlayer = null;
//
//         tiles.Clear();
//
//         foreach (var unit in units.Values.ToArray())
//             unit.Dispose();
//         units.Clear();
//
//         foreach (var building in buildings.Values.ToArray())
//             building.Dispose();
//         buildings.Clear();
//
//         foreach (var set in triggers.Values)
//             set.Clear();
//
//         foreach (var bridge in bridges)
//             bridge.view.bridge = null;
//         bridges.Clear();
//
//         RebuildTilemapMesh();
//     }
//
//     public class ReadingOptions {
//         public bool clearGame = true;
//         public string saveName;
//         public string input = "";
//         public bool spawnBuildingViews = true;
//         public bool checkLocalPlayerIsSet = true;
//         public bool selectExistingPlayersInsteadOfCreatingNewOnes = false;
//         public Vector2Int transform = Vector2Int.one;
//         public bool loadCameraRig = true;
//     }
//
//     public void LoadInternal(ReadingOptions options) {
//
//         if (options.clearGame)
//             Clear();
//
//         string input;
//         if (options.saveName != null) {
//             var found = TryGetLatestSaveFilePath(options.saveName, out var filePath);
//             Assert.IsTrue(found, options.saveName);
//             Debug.Log($"Reading from: {filePath}");
//             input = File.ReadAllText(filePath);
//         }
//         else
//             input = options.input;
//
//         LevelReader.ReadInto(this, input, options.transform, options.spawnBuildingViews, options.selectExistingPlayersInsteadOfCreatingNewOnes,
//             options.loadCameraRig);
//
//         player = players.Count == 0 ? null : players[0];
//
//         RebuildTilemapMesh();
//
//         if (options.checkLocalPlayerIsSet && players.Count > 0)
//             Assert.IsNotNull(localPlayer, "local player is not set");
//     }
//
//     [Command]
//     public void LoadTransformed(string name, Vector2Int transform) {
//         LoadInternal(new ReadingOptions { saveName = name, transform = transform });
//     }
//
//     [Command]
//     public void Load(string name) {
//         LoadInternal(new ReadingOptions { saveName = name });
//     }
//     [Command]
//     public void LoadAdditively(string name) {
//         LoadInternal(new ReadingOptions {
//             saveName = name,
//             clearGame = false,
//             selectExistingPlayersInsteadOfCreatingNewOnes = true,
//             loadCameraRig = false
//         });
//     }
//     [Command]
//     public void LoadFromText(string text) {
//         LoadInternal(new ReadingOptions { input = text });
//     }
//     [Command]
//     public void LoadFromTextAdditively(string text) {
//         LoadInternal(new ReadingOptions {
//             input = text,
//             clearGame = false,
//             selectExistingPlayersInsteadOfCreatingNewOnes = true,
//             loadCameraRig = false
//         });
//     }
//
//     [Command]
//     public void OpenSaveFile(string name) {
//         var found = TryGetLatestSaveFilePath(name, out var filePath);
//
//         Assert.IsTrue(found);
//         ProcessStartInfo startInfo = new ProcessStartInfo("/usr/local/bin/subl");
//         startInfo.WindowStyle = ProcessWindowStyle.Normal;
//         startInfo.Arguments = '"' + filePath + '"';
//
//         Process.Start(startInfo);
//     }
//
//     [Command]
//     public void PopSaveFile(string name) {
//         if (TryGetLatestSaveFilePath(name, out var filePath)) {
//             File.Delete(filePath);
//             if (File.Exists(filePath + ".meta"))
//                 File.Delete(filePath + ".meta");
//         }
//     }
// }
//
//
// [Serializable]
// public class TriggerNameColorDictionary : SerializableDictionary<TriggerName, Color> { }