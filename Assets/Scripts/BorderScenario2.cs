using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BorderScenario2 : MonoBehaviour {

    public Game game;
    public StateMachine stateMachine;
    public CameraRig cameraRig;
    public TMP_Text locationAndTimeText;
    public float locationAndTimeTextSpeed = 15;
    public float lightTankSpeed = 2.5f;
    public float reconSpeed = 2.5f;
    public float infantrySpeed = 1.5f;
    public float defaultUnitSpeed = 2.5f;
    public bool actuallyMoveUnits = false;

    public List<IEnumerator> coroutines = new();
    public List<IEnumerator> newCoroutines = new();

    public void Start() {
        game = FindObjectOfType<Game>();
        Assert.IsTrue(game);
        stateMachine = game.stateMachine;
        cameraRig = FindObjectOfType<CameraRig>();
        Assert.IsTrue(cameraRig);
    }

    [FormerlySerializedAs("unitPaths")] public List<string> blueUnitPaths = new() {
        "Tank1",
        "Tank2",
        "Tank3",
        "Recon",
        "Infantry1",
        "Infantry2",
        "Infantry3",
        "Recon2"
    };
    public List<string> redUnitsPaths = new() {
        "RedTank1",
        "RedTank2",
    };
    public List<string> redUnitsPaths2 = new() {
        "RedTank3",
        "RedTank4",
    };
    public List<string> redAttackPaths = new() {
        "RedRocket1",
        "RedRocket2",
    };

    [Command]
    public void Play() {
        coroutines.Clear();
        coroutines.Add(Animation());
    }

    public IEnumerator Animation() {
        var level = game.Level;
        var bluePlayer = level.FindPlayer(ColorName.Blue);

        foreach (var player in level.players)
            player.view.Hide();
        level.view.tilemapCursor.Hide();

        PostProcessing.ColorFilter = Color.black;
        coroutines.Add(LocationAndTimeTextTypingAndFadeOut(15, 3, .5f));

        var time = Time.time;
        while (Time.time < time + 3)
            yield return null;

        coroutines.Add(CameraFade(Color.black, Color.white, 1));

        var blueUnitsMovements = new List<IEnumerator>();
        foreach (var pathName in blueUnitPaths) {
            var path = level.FindPath(pathName);
            var coroutine = UnitMovement(path.ToList());
            blueUnitsMovements.Add(coroutine);
            coroutines.Add(coroutine);
        }

        var cameraIntroPath = level.FindPath("CameraIntro");
        if (cameraIntroPath is { Count: > 0 }) {
            cameraRig.transform.position = cameraIntroPath.First.Value.ToVector3();
            var introCameraMovement = CameraMovement(cameraIntroPath.ToList(), 3);
            while (introCameraMovement.MoveNext())
                yield return null;
        }

        while (blueUnitsMovements.Any(c => coroutines.Contains(c)))
            yield return null;

        var infantryPath = level.FindPath("Infantry");
        var infantry = new Unit {
            Player = bluePlayer,
            type = UnitType.Infantry,
            Position = infantryPath.First.Value,
            lookDirection = Vector2Int.up
        };
        infantry.Initialize();
        var infantryMovement = UnitMovement(infantryPath.ToList());
        while (infantryMovement.MoveNext())
            yield return null;

        stateMachine.Push(new BorderIncidentIntroDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentIntroDialogueState>() != null)
            yield return null;

        var cameraRocketeerPath = level.FindPath("CameraRocketeers");
        if (cameraRocketeerPath is { Count: > 0 }) {
            var redRocketeersCameraMovement = CameraMovement(cameraRocketeerPath.ToList(), 5);
            while (redRocketeersCameraMovement.MoveNext())
                yield return null;
        }

        stateMachine.Push(new BorderIncidentRedRocketeersDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentRedRocketeersDialogueState>() != null)
            yield return null;

        // top red rocketeeer
        {
            var waypoints = level.FindPath("RedRocketeerTop");
            Assert.IsTrue(level.TryGetUnit(waypoints.First.Value, out var rocketeer));
            Assert.IsTrue(level.TryGetBuilding(waypoints.First.Next.Value, out var missileSilo));

            while (stateMachine.TryFind<SelectionState>() == null)
                yield return null;
            game.EnqueueCommand(SelectionState.Command.Select, waypoints.First.Value);
            while (stateMachine.TryFind<PathSelectionState>() == null)
                yield return null;
            game.EnqueueCommand(PathSelectionState.Command.ReconstructPath, waypoints.First.Next.Value);
            game.EnqueueCommand(PathSelectionState.Command.Move);
            while (stateMachine.TryFind<ActionSelectionState>() == null)
                yield return null;
            game.EnqueueCommand(ActionSelectionState.Command.Execute, new UnitAction(UnitActionType.LaunchMissile, rocketeer, stateMachine.Find<PathSelectionState>().path, targetBuilding: missileSilo));
            while (stateMachine.TryFind<MissileTargetSelectionState>() == null)
                yield return null;
            stateMachine.Peek<MissileTargetSelectionState>().AimPosition = waypoints.Last.Value;
            time = Time.time;
            while (Time.time < time + 1)
                yield return null;
            game.EnqueueCommand(MissileTargetSelectionState.Command.LaunchMissile, waypoints.Last.Value);
            while (stateMachine.TryPeek<SelectionState>() == null)
                yield return null;
        }

        // bottom red rocketeeer
        {
            var waypoints = level.FindPath("RedRocketeerBottom");
            Assert.IsTrue(level.TryGetUnit(waypoints.First.Value, out var rocketeer));
            Assert.IsTrue(level.TryGetBuilding(waypoints.First.Next.Value, out var missileSilo));

            while (stateMachine.TryFind<SelectionState>() == null)
                yield return null;
            game.EnqueueCommand(SelectionState.Command.Select, waypoints.First.Value);
            while (stateMachine.TryFind<PathSelectionState>() == null)
                yield return null;
            game.EnqueueCommand(PathSelectionState.Command.ReconstructPath, waypoints.First.Next.Value);
            game.EnqueueCommand(PathSelectionState.Command.Move);
            while (stateMachine.TryFind<ActionSelectionState>() == null)
                yield return null;
            game.EnqueueCommand(ActionSelectionState.Command.Execute, new UnitAction(UnitActionType.LaunchMissile, rocketeer, stateMachine.Find<PathSelectionState>().path, targetBuilding: missileSilo));
            while (stateMachine.TryFind<MissileTargetSelectionState>() == null)
                yield return null;
            stateMachine.Peek<MissileTargetSelectionState>().AimPosition = waypoints.Last.Value;
            time = Time.time;
            while (Time.time < time + 1)
                yield return null;
            game.EnqueueCommand(MissileTargetSelectionState.Command.LaunchMissile, waypoints.Last.Value);
            while (stateMachine.TryPeek<SelectionState>() == null)
                yield return null;
        }

        var redUnitsMovements = new List<IEnumerator>();
        foreach (var pathName in redUnitsPaths) {
            var path = level.FindPath(pathName);
            var coroutine = UnitMovement(path.ToList(), rotateToPlayerDirection: true);
            redUnitsMovements.Add(coroutine);
            coroutines.Add(coroutine);
        }
        while (redUnitsMovements.Any(c => coroutines.Contains(c)))
            yield return null;

        stateMachine.Push(new BorderIncidentWhatIsHappeningDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentWhatIsHappeningDialogueState>() != null)
            yield return null;
        
        foreach (var path in redAttackPaths) {
            var coroutine = Attack(level.FindPath(path));
            while (coroutine.MoveNext())
                yield return null;
            time = Time.time;
            while (Time.time < time + .5f)
                yield return null;
        }

        foreach (var unit in level.units.Values)
            unit.Moved = false;

        var redUnitsMovements2 = new List<IEnumerator>();
        foreach (var pathName in redUnitsPaths2) {
            var path = level.FindPath(pathName);
            var coroutine = UnitMovement(path.ToList(), rotateToPlayerDirection: true);
            redUnitsMovements2.Add(coroutine);
            coroutines.Add(coroutine);
        }
        
        var cameraEndPath = level.FindPath("CameraEnd");
        var cameraMovement = CameraMovement(cameraEndPath.ToList(), 1);
        while (cameraMovement.MoveNext())
            yield return null;
        
        while (redUnitsMovements2.Any(c => coroutines.Contains(c)))
            yield return null;
    }

    public IEnumerator Attack(Level.Path waypoints) {
        
        var level = game.Level;
        Assert.IsTrue(level.TryGetUnit(waypoints.First.Value, out var attacker));
        Assert.IsTrue(level.TryGetUnit(waypoints.Last.Value, out var target));

        var weaponName = attacker.type switch {
            UnitType.LightTank or UnitType.MediumTank or UnitType.Artillery => WeaponName.Cannon,
            UnitType.Recon => WeaponName.MachineGun,
            UnitType.Infantry => WeaponName.Rifle,
            UnitType.Rockets => WeaponName.RocketLauncher,
            _ => throw new ArgumentOutOfRangeException(attacker.type.ToString())
        };

        while (stateMachine.TryFind<SelectionState>() == null)
            yield return null;
        game.EnqueueCommand(SelectionState.Command.Select, attacker.NonNullPosition);
        while (stateMachine.TryFind<PathSelectionState>() == null)
            yield return null;
        if (waypoints.Count > 2) 
            game.EnqueueCommand(PathSelectionState.Command.ReconstructPath, waypoints.First.Next.Value);
        game.EnqueueCommand(PathSelectionState.Command.Move);
        while (stateMachine.TryFind<ActionSelectionState>() == null)
            yield return null;
        game.EnqueueCommand(ActionSelectionState.Command.Execute, new UnitAction(UnitActionType.Attack, attacker, stateMachine.Find<PathSelectionState>().path, targetUnit: target, weaponName: weaponName));
        while (stateMachine.TryPeek<SelectionState>() == null)
            yield return null;
    }

    public IEnumerator LocationAndTimeTextTypingAndFadeOut(float typingSpeed, float holdDuration, float fadeDuration) {
        var originalColor = locationAndTimeText.color;
        locationAndTimeText.enabled = true;
        var originalText = locationAndTimeText.text;
        var time = Time.time;
        var typingDuration = originalText.Length / locationAndTimeTextSpeed;
        while (Time.time < time + typingDuration) {
            var t = (Time.time - time) / typingDuration;
            var substringLength = (int)(t * originalText.Length);
            locationAndTimeText.text = originalText[..substringLength];
            yield return null;
        }

        locationAndTimeText.text = originalText;

        time = Time.time;
        while (Time.time < time + holdDuration)
            yield return null;

        locationAndTimeText.text = originalText;
        time = Time.time;
        while (Time.time < time + fadeDuration) {
            var t = (Time.time - time) / fadeDuration;
            locationAndTimeText.color = originalColor * new Color(1, 1, 1, 1 - t);
            yield return null;
        }

        locationAndTimeText.enabled = false;
        locationAndTimeText.color = originalColor;
    }

    public IEnumerator CameraFade(Color from, Color to, float duration) {
        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            PostProcessing.ColorFilter = Color.Lerp(from, to, t);
            yield return null;
        }

        PostProcessing.ColorFilter = to;
    }

    public IEnumerator CameraMovement(List<Vector2Int> waypoints, float speed) {
        var segments = new List<Segment>();
        var accumulatedLength = 0f;
        for (var i = -1; i < waypoints.Count - 1; i++) {
            var start = i == -1 ? cameraRig.transform.position.ToVector2Int() : waypoints[i];
            var end = waypoints[i + 1];
            var length = Vector2.Distance(start, end);
            var segment = new Segment() {
                startLength = accumulatedLength,
                endLength = accumulatedLength += length,
                startPosition = start,
                endPosition = end
            };
            if (length > .001f)
                segments.Add(segment);
        }

        {
            var length = 0f;
            while (length < accumulatedLength) {
                var segment = segments.Find(s => s.startLength <= length && length <= s.endLength);
                var t = (length - segment.startLength) / (segment.endLength - segment.startLength);
                var position = Vector3.Lerp(segment.startPosition.ToVector3(), segment.endPosition.ToVector3(), t);
                cameraRig.transform.position = position;

                length += speed * Time.deltaTime;
                yield return null;
            }
        }
    }

    public IEnumerator UnitMovement(List<Vector2Int> waypoints, bool rotateToPlayerDirection = true) {
        if (waypoints.Count == 0)
            yield break;
        var position = waypoints[0];
        var found = game.Level.TryGetUnit(position, out var unit);
        Assert.IsTrue(found, $"Unit not found at {position}");
        var unitView = unit.view;
        Assert.IsTrue(unitView);
        unitView.Position = position;
        var path = new List<Vector2Int> { position };
        var speed = unitView.prefab.name switch {
            "WbLightTank" => lightTankSpeed,
            "WbRecon" => reconSpeed,
            "WbInfantry" => infantrySpeed,
            _ => defaultUnitSpeed
        };
        foreach (var waypoint in waypoints)
            path.AddRange(Woo.Traverse2D(path[^1], waypoint));
        var oldEnableDance = unitView.enableDance;
        unitView.enableDance = false;
        var animation = new MoveSequence(unitView.transform, path, speed, _finalDirection: rotateToPlayerDirection ? unit.Player.unitLookDirection : null).Animation();
        while (animation.MoveNext())
            yield return null;
        unitView.enableDance = oldEnableDance;
        if (actuallyMoveUnits)
            unit.Position = path[^1];
    }

    public void Update() {
        newCoroutines.Clear();
        for (var i = 0; i < coroutines.Count; i++) {
            var coroutine = coroutines[i];
            if (coroutine.MoveNext())
                newCoroutines.Add(coroutine);
        }
        (coroutines, newCoroutines) = (newCoroutines, coroutines);
    }

    public struct Segment {
        public float startLength, endLength;
        public Vector2 startPosition, endPosition;
    }
}