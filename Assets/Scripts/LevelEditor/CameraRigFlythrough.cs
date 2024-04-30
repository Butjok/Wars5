using System.Collections;
using System.Collections.Generic;
using System.IO;
using Butjok.CommandLine;
using Stable;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraRigFlythrough : MonoBehaviour {

    public CameraRig cameraRig;
    public List<Vector3> waypoints = new();

    public void Reset() {
        cameraRig = GetComponent<CameraRig>();
        Assert.IsTrue(cameraRig);
    }

    [Command]
    public void Clear() {
        waypoints.Clear();
    }
    [Command]
    public void Add() {
        waypoints.Add(cameraRig.transform.position);
    }
    [Command]
    public void Play(string slotName) {
        if (TryLoad(slotName)) {
            StopAllCoroutines();
            StartCoroutine(Animation());
        }
    }

    public static string GetFullSlotName(string slotName) {
        return nameof(CameraRigFlythrough) + '.' + slotName;
    }

    [Command]
    public void Save(string slotName) {
        var fullSlotName = GetFullSlotName(slotName);
        var tw = new StringWriter();
        tw.PostfixWriteLine("set-speed ( {0} )", speed);
        foreach (var waypoint in waypoints)
            tw.PostfixWriteLine("add ( {0} )", waypoint);
        var text = tw.ToString();
        PlayerPrefs.SetString(fullSlotName, text);
        Debug.Log(text);
    }

    [Command]
    public bool TryLoad(string slotName) {
        var fullSlotName = GetFullSlotName(slotName);
        var text = PlayerPrefs.GetString(fullSlotName);
        if (text == null)
            return false;
        var stack = new Stack();
        waypoints.Clear();
        foreach (var token in Tokenizer.Tokenize(text.ToPostfix()))
            switch (token) {
                case "add":
                    var position = (Vector3)stack.Pop();
                    waypoints.Add(position);
                    break;
                case "set-speed":
                    speed = (dynamic)stack.Pop();
                    break;
                default:
                    stack.ExecuteToken(token);
                    break;
            }

        return true;
    }

    public struct Segment {
        public float startLength, endLength;
        public Vector3 startPosition, endPosition;
    }

    [Command]
    public float speed = 1;

    public IEnumerator Animation(string slotName) {
        if (TryLoad(slotName)) {
            StopAllCoroutines();
            return Animation();
        }
        return null;
    }

    public IEnumerator Animation() {
        var segments = new List<Segment>();
        var accumulatedLength = 0f;
        for (var i = 0; i < waypoints.Count - 1; i++) {
            var start = waypoints[i];
            var end = waypoints[i + 1];
            var length = Vector3.Distance(start, end);
            var segment = new Segment() {
                startLength = accumulatedLength,
                endLength = accumulatedLength += length,
                startPosition = start,
                endPosition = end
            };
            segments.Add(segment);
        }

        {
            var length = 0f;
            while (length < accumulatedLength) {
                var segment = segments.Find(s => s.startLength <= length && length <= s.endLength);
                var t = (length - segment.startLength) / (segment.endLength - segment.startLength);
                var position = Vector3.Lerp(segment.startPosition, segment.endPosition, t);
                cameraRig.transform.position = position;

                length += speed * Time.deltaTime;
                yield return null;
            }
        }
    }
}