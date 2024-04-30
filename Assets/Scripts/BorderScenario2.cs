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
    public WaypointList introCameraWaypoints;
    public WaypointList redRocketeersCameraWaypoints;
    public List<WaypointList> cameraWaypointLists = new();
    public int cameraWaypointListIndex = 0;

    public List<IEnumerator> coroutines = new();
    public List<IEnumerator> newCoroutines = new();

    public bool showWaypoints = true;
    public WaypointList CurrentWaypointList => cameraWaypointLists[cameraWaypointListIndex];

    public void Start() {
        game = FindObjectOfType<Game>();
        Assert.IsTrue(game);
        stateMachine = game.stateMachine;
        cameraRig = FindObjectOfType<CameraRig>();
        Assert.IsTrue(cameraRig);
        cameraWaypointLists = new List<WaypointList> {
            introCameraWaypoints,
            redRocketeersCameraWaypoints
        };
    }

    [Command]
    public void Play() {
        coroutines.Clear();
        coroutines.Add(Animation());
    }

    [Command]
    public void ClearWaypoints() {
        CurrentWaypointList.waypoints.Clear();
    }
    [Command]
    public void AddWaypoint() {
        CurrentWaypointList.waypoints.Add(cameraRig.transform.position.ToVector2Int());
    }
    [Command]
    public void MoveCamera(float speed) {
        coroutines.Clear();
        coroutines.Add(CameraMovement(CurrentWaypointList.waypoints, speed));
    }

    public IEnumerator Animation() {
        foreach (var player in game.Level.players)
            player.view.Hide();
        game.Level.view.tilemapCursor.Hide();

        PostProcessing.ColorFilter = Color.black;
        coroutines.Add(LocationAndTimeTextTypingAndFadeOut(15, 3, .5f));

        var time = Time.time;
        while (Time.time < time + 3)
            yield return null;

        coroutines.Add(CameraFade(Color.black, Color.white, 1));

        if (introCameraWaypoints.waypoints.Count > 0)
            cameraRig.transform.position = introCameraWaypoints.waypoints[0].ToVector3();
        var introCameraMovement = CameraMovement(introCameraWaypoints.waypoints, 3);
        coroutines.Add(introCameraMovement);
        while (coroutines.Contains(introCameraMovement))
            yield return null;

        stateMachine.Push(new BorderIncidentIntroDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentIntroDialogueState>() != null)
            yield return null;

        var redRocketeersCameraMovement = CameraMovement(redRocketeersCameraWaypoints.waypoints, 5);
        coroutines.Add(redRocketeersCameraMovement);
        while (coroutines.Contains(redRocketeersCameraMovement))
            yield return null;

        stateMachine.Push(new BorderIncidentRedRocketeersDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentRedRocketeersDialogueState>() != null)
            yield return null;

        stateMachine.Push(new BorderIncidentWhatIsHappeningDialogueState(stateMachine));
        while (stateMachine.TryFind<BorderIncidentWhatIsHappeningDialogueState>() != null)
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
    
    public IEnumerator UnitMovement(List<Vector2Int> waypoints, float speed) {
        if (waypoints.Count == 0)
            yield break;
        var position = waypoints[0];
        var found = game.Level.TryGetUnit(position, out var unit);
        Assert.IsTrue(found, $"Unit not found at {position}");
        var unitView = unit.view;
        Assert.IsTrue(unitView);
        unitView.Position = position;
        var path = new List<Vector2Int>{position};
        foreach (var waypoint in waypoints)
            path.AddRange(Woo.Traverse2D(path[^1], waypoint));
        var animation = new MoveSequence(unitView.transform, path, speed).Animation();
        while (animation.MoveNext())
            yield return null;
        unit.Position = path[^1];
    }

    public void Update() {
        newCoroutines.Clear();
        for (var i = 0; i < coroutines.Count; i++) {
            var coroutine = coroutines[i];
            if (coroutine.MoveNext())
                newCoroutines.Add(coroutine);
            //else
            //Debug.Log($"Coroutine {coroutine.GetType()} finished");
        }

        (coroutines, newCoroutines) = (newCoroutines, coroutines);

        if (showWaypoints) {
            var points = CurrentWaypointList.waypoints;
            for (var i = 0; i < points.Count; i++) {
                var point = points[i];
                Draw.ingame.CircleXZ(point.ToVector3(), 0.25f, Color.yellow);
                if (i == 0)
                    Draw.ingame.Label3D(point.ToVector3(), quaternion.identity, '\n' + CurrentWaypointList.name, .25f, LabelAlignment.Center, Color.yellow);
                Draw.ingame.Label3D(point.ToVector3(), quaternion.identity, i.ToString(), .25f, LabelAlignment.Center, Color.yellow);
                if (i > 0)
                    Draw.ingame.Line(points[i - 1].ToVector3(), point.ToVector3(), Color.yellow);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
            cameraWaypointListIndex = (cameraWaypointListIndex + 1) % cameraWaypointLists.Count;
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        GUILayout.Label($"\n\nWaypoint List: {CurrentWaypointList.name}");
    }

    public struct Segment {
        public float startLength, endLength;
        public Vector2 startPosition, endPosition;
    }
}