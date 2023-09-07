using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class TilemapCursor : MonoBehaviour {

    public GameObject viewRoot;
    public Func<Vector2Int, bool> isValidPosition = _ => true;
    public TMP_Text text;
    public float repeatCoolDown = 5;
    public float repeatDelay = .25f;

    private Vector2Int? position;
    public bool TryGetPosition(out Vector2Int result) {
        if (position is { } value) {
            result = value;
            return true;
        }
        result = default;
        return false;
    }

    [Command]
    public bool TrySetPosition(Vector2Int value) {
        if (!isValidPosition(value))
            return false;
        var oldPosition = position;
        position = value;
        if ((oldPosition == null) != (position == null)) {
            viewRoot.SetActive(position != null);
            if (text)
                text.enabled = viewRoot.activeSelf;
        }
        return true;
    }

    public static readonly Dictionary<KeyCode, Vector2Int> bindings = new() {
        [KeyCode.UpArrow] = Vector2Int.up,
        [KeyCode.DownArrow] = Vector2Int.down,
        [KeyCode.LeftArrow] = Vector2Int.left,
        [KeyCode.RightArrow] = Vector2Int.right
    };
    public IEnumerator repeatLoop;

    public Vector2Int CalculateOffset(Func<KeyCode, bool> predicate) {
        var result = Vector2Int.zero;
        foreach (var (key, offset) in bindings)
            if (predicate(key))
                result += offset;
        return result;
    }

    public const string horizontalAxisName = "TilemapCursorHorizontal";
    public const string verticalAxisName = "TilemapCursorVertical";

    public static Vector2Int GetOffset(Vector2Int axisMask) {
        return new Vector2Int(Mathf.RoundToInt(Input.GetAxisRaw(horizontalAxisName)), Mathf.RoundToInt(Input.GetAxisRaw(verticalAxisName))) * axisMask;
    }

    public IEnumerator RepeatLoop(Vector2Int axisMask) {

        if (TryGetPosition(out var startPosition) && TrySetPosition(startPosition + GetOffset(axisMask))) {

            var startTime = Time.time;
            yield return new WaitWhile(() => Time.time < startTime + repeatDelay && GetOffset(axisMask) != Vector2Int.zero);

            Vector2Int offset;
            while ((offset = GetOffset(axisMask)) != Vector2Int.zero && TryGetPosition(out var position) && TrySetPosition(position + offset)) {
                startTime = Time.time;
                var coolDown = repeatCoolDown / cameraRig.ToWorldUnits(1);
                yield return new WaitWhile(() => Time.time < startTime + coolDown && GetOffset(axisMask) != Vector2Int.zero);
            }
        }

        if (axisMask.x != 0)
            horizontalRepeatLoop = null;
        else
            verticalRepeatLoop = null;
    }

    public int horizontalTickFrame, verticalTickFrame;
    public IEnumerator horizontalRepeatLoop, verticalRepeatLoop;

    public void Update() {
        if (horizontalRepeatLoop == null && GetOffset(Vector2Int.right) != Vector2Int.zero) {
            horizontalRepeatLoop = RepeatLoop(Vector2Int.right);
            StartCoroutine(horizontalRepeatLoop);
        }
        if (verticalRepeatLoop == null && GetOffset(Vector2Int.up) != Vector2Int.zero) {
            verticalRepeatLoop = RepeatLoop(Vector2Int.up);
            StartCoroutine(verticalRepeatLoop);
        }

        if (viewRoot.activeSelf && TryGetPosition(out var position)) {
            viewRoot.transform.position = Vector3.Lerp(viewRoot.transform.position, position.ToVector3(),
                speed * Time.deltaTime / Vector2.Distance(viewRoot.transform.position.ToVector2(), position));
        }
    }

    public CameraRig cameraRig;
    public float speed = .5f;
}