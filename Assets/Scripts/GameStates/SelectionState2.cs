using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerTurnState : IDisposableState {

	public Level level;
	public bool animateTurnStart;
	public float startTime;
	public PlayerTurnState(Level level, bool animateTurnStart = true) {
		this.level = level;
		this.animateTurnStart = animateTurnStart;
	}

	public IEnumerator<StateChange> Run {
		get {
			startTime = Time.unscaledTime;
			var player = level.CurrentPlayer;

			var day = level.Day();

			if (TurnButton.TryGet(out var turnButton)) {
				turnButton.Color = player.Color;
				if (animateTurnStart && PersistentData.Loaded.gameSettings.animateNight && turnButton.Day is {} shownDay && shownDay != day) {
					var isCompleted = turnButton.PlayAnimation(day);
					while (!isCompleted())
						yield return StateChange.none;
				}
				else
					turnButton.Day = day;
			}

			if (DayText.TryFind(out var dayText) && animateTurnStart) {
				var isCompleted = dayText.PlayAnimation(day, player.Color);
				while (!isCompleted())
					yield return StateChange.none;
			}

			if (MusicPlayer.TryGet(out var musicPlayer)) {
				var themes = Persons.GetMusicThemes(player.coName);
				if (themes.Count > 0)
					musicPlayer.StartPlaying(themes);
				else
					musicPlayer.StopPlaying();
			}

			player.view2.Show();

			if (player.abilityActivationTurn != null && level.turn != player.abilityActivationTurn)
				yield return StateChange.Push(new AbilityDeactivationState(level));

			yield return StateChange.Push(new SelectionState2(level));
		}
	}

	public void Dispose() {
		level.CurrentPlayer.view2.Hide();
		MusicPlayer.TryGet()?.StopPlaying();

		var turnLength = Time.unscaledTime - startTime;
	}
}

public class SelectionState2 : IDisposableState {

	public const string prefix = "selection-state.";

	public const string endTurn = prefix + "end-turn";
	public const string openGameMenu = prefix + "open-game-menu";
	public const string exitToLevelEditor = prefix + "exit-to-level-editor";
	public const string cyclePositions = prefix + "cycle-positions";
	public const string select = prefix + "select";
	public const string triggerVictory = prefix + "trigger-victory";
	public const string triggerDefeat = prefix + "trigger-defeat";
	public const string useAbility = prefix + "use-ability";

	public bool shouldEndTurn;

	public struct PreselectionPosition {
		public Unit unit;
		public Building building;
		public int Priority => unit != null ? 1 : 0;
		public Vector3 Position => (unit?.NonNullPosition ?? building.position).Raycast();
		public Sprite Thumbnail => null;
	}

	public Level level;
	public SelectionState2(Level level) {
		this.level = level;
	}

	public IEnumerator<StateChange> Run {
		get {
			var player = level.CurrentPlayer;

			var unmovedUnits = level.units.Values
				.Where(unit => unit.Player == player && !unit.Moved)
				.ToList();

			var accessibleBuildings = level.buildings.Values
				.Where(building => building.Player == player &&
				                   Rules.GetBuildableUnitTypes(building).Any() &&
				                   !level.TryGetUnit(building.position, out var _))
				.ToList();

			List<PreselectionPosition> positions = null;
			if (PreselectionCursor.TryFind(out var preselectionCursor)) {

				var enumeration = unmovedUnits.Select(unit => new PreselectionPosition { unit = unit })
					.Concat(accessibleBuildings.Select(building => new PreselectionPosition { building = building }))
					.OrderByDescending(position => position.Priority);

				if (CameraRig.TryFind(out var cameraRig))
					enumeration = enumeration.ThenBy(position => Vector3.Distance(cameraRig.transform.position, position.Position));

				positions = enumeration.ToList();
			}
			var positionIndex = -1;

			if (CursorView.TryFind(out var cursorView) && !level.autoplay && !level.CurrentPlayer.IsAi)
				cursorView.show = true;

			if (TurnButton.TryGet(out var turnButton))
				turnButton.Interactable = true;

			var issuedAiCommands = false;
			while (true) {
				yield return StateChange.none;

				if (level.autoplay || Input.GetKey(KeyCode.Alpha8)) {
					if (!issuedAiCommands) {
						issuedAiCommands = true;
						level.IssueAiCommandsForSelectionState();
					}
				}
				else if (!level.CurrentPlayer.IsAi) {

					if (shouldEndTurn || Input.GetKeyDown(KeyCode.F2)) {
						shouldEndTurn = false;
						level.commands.Enqueue(endTurn);
					}

					//else if ((Input.GetKeyDown(KeyCode.Escape)) && (!preselectionCursor || !preselectionCursor.Visible))
					//    main.commands.Enqueue(openGameMenu);

					else if (Input.GetKeyDown(KeyCode.F5) && level is LevelEditor)
						level.commands.Enqueue(exitToLevelEditor);

					else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(Mouse.right)) && preselectionCursor && preselectionCursor.Visible)
						preselectionCursor.Hide();

					else if (Input.GetKeyDown(KeyCode.Tab)) {
						level.stack.Push(Input.GetKey(KeyCode.LeftShift) ? -1 : 1);
						level.commands.Enqueue(cyclePositions);
					}

					else if (Input.GetKeyDown(KeyCode.Space) && preselectionCursor.Visible) {
						level.stack.Push(preselectionCursor.transform.position.ToVector2().RoundToInt());
						level.commands.Enqueue(select);
					}

					else if ((Input.GetMouseButtonDown(Mouse.left) || Input.GetKeyDown(KeyCode.Space)) && Mouse.TryGetPosition(out Vector2Int mousePosition)) {
						level.stack.Push(mousePosition);
						level.commands.Enqueue(select);
					}

					else if (Input.GetKeyDown(KeyCode.F6) && Rules.CanUseAbility(level.CurrentPlayer))
						level.commands.Enqueue(useAbility);
				}

				while (level.commands.TryDequeue(out var input))
					foreach (var token in Tokenizer.Tokenize(input)) {
						switch (token) {

							case select: {
								var position = level.stack.Pop<Vector2Int>();

								if (level.TryGetUnit(position, out var unit)) {
									if (unit.Player != player || unit.Moved)
										UiSound2.PlayOneShot(UiClipName.NotAllowed);
									else {
										unit.view.Selected = true;
										yield return StateChange.ReplaceWith(nameof(PathSelectionState), PathSelectionState.Run(level, unit));
									}
								}

								else if (level.TryGetBuilding(position, out var building) &&
								         Rules.GetBuildableUnitTypes(building).Any()) {
									if (building.Player != player)
										UiSound2.PlayOneShot(UiClipName.NotAllowed);
									else
										yield return StateChange.ReplaceWith(nameof(UnitBuildState), UnitBuildState.New(level, building));
								}

								break;
							}

							case endTurn:

								level.turn++;

								foreach (var unit in level.units.Values)
									unit.Moved = false;

								yield return StateChange.PopThenPush(2, new PlayerTurnState(level));
								break;

							case openGameMenu:
								yield return StateChange.ReplaceWith(nameof(GameMenuState), GameMenuState.Run(level));
								break;

							case exitToLevelEditor:
								yield return StateChange.Pop(2);
								break;

							case cyclePositions: {
								var offset = level.stack.Pop<int>();
								if (preselectionCursor && positions.Count > 0) {
									positionIndex = (positionIndex + offset).PositiveModulo(positions.Count);
									var position = positions[positionIndex];
									if (preselectionCursor)
										preselectionCursor.ShowAt(position.Position, position.Thumbnail);
								}
								break;
							}

							case useAbility: {
								if (Rules.CanUseAbility(player))
									yield return StateChange.Push(new AbilityActivationState(level));
								else
									UiSound2.PlayOneShot(UiClipName.NotAllowed);
								break;
							}

							case triggerVictory:
								yield return StateChange.ReplaceWith(nameof(VictoryDefeatState.Victory), VictoryDefeatState.Victory(level));
								break;

							case triggerDefeat:
								yield return StateChange.ReplaceWith(nameof(VictoryDefeatState.Defeat), VictoryDefeatState.Defeat(level));
								break;

							default:
								level.stack.ExecuteToken(token);
								break;
						}
					}
			}
		}
	}

	public void Dispose() {
		if (CursorView.TryFind(out var cursorView))
			cursorView.show = false;
		if (PreselectionCursor.TryFind(out var preselectionCursor))
			preselectionCursor.Hide();
		if (TurnButton.TryGet(out var turnButton))
			turnButton.Interactable = false;
	}
}

public class AbilityActivationState : IDisposableState {

	public Level level;
	public AbilityActivationState(Level level) {
		this.level = level;
	}

	public IEnumerator<StateChange> Run {
		get {
			var player = level.CurrentPlayer;

			Assert.IsTrue(Rules.CanUseAbility(player));
			Assert.IsTrue(!Rules.AbilityInUse(player));

			player.abilityActivationTurn = level.turn;
			player.SetAbilityMeter(0, false);

			Debug.Log($"starting ability of {player}");

			switch (player.coName) {
				case PersonName.Natalie:
					yield return StateChange.Push(nameof(NatalieAbility), NatalieAbility);
					break;
				case PersonName.Vladan:
					yield return StateChange.Push(nameof(NatalieAbility), NatalieAbility);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	private IEnumerator<StateChange> NatalieAbility {
		get {
			var player = level.CurrentPlayer;

			if (!CameraRig.TryFind(out var cameraRig))
				yield break;

			var units = level.FindUnitsOf(player);
			foreach (var unit in units) {
				cameraRig.Jump(unit.NonNullPosition.Raycast());
				while (cameraRig.JumpCoroutine != null)
					yield return StateChange.none;
				unit.SetHp(unit.Hp + 2);
			}
		}
	}

	public void Dispose() { }
}

public class AbilityDeactivationState : IDisposableState {

	public Level level;
	public AbilityDeactivationState(Level level) {
		this.level = level;
	}

	public IEnumerator<StateChange> Run {
		get {
			var player = level.CurrentPlayer;

			Assert.IsTrue(Rules.AbilityInUse(player));

			player.abilityActivationTurn = null;
			Debug.Log($"stopping ability of {player}");

			yield break;
		}
	}

	public void Dispose() { }
}