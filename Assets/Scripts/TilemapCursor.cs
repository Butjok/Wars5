using System;
using System.Collections;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TilemapCursor : MonoBehaviour {

    public const int right = 0, up = 1;

    public GameObject viewRoot;
    public Func<Vector2Int, bool> isValidPosition = _ => true;
    public TMP_Text text;
    public float repeatCoolDown = 5;
    public float repeatDelay = .25f;
    public static readonly string[] axisNames = { "TilemapCursorHorizontal", "TilemapCursorVertical" };
    public IEnumerator[] repeatLoops = new IEnumerator[2];
    public CameraRig cameraRig;
    public float speed = .5f;
    public bool enableKeyboard = false;
    public bool enableMouse = true;

    public Image tileThumbnail;
    public Image unitThumbnail;

    public Sprite[] tileThumbnails = { };
    public Sprite[] unitThumbnails = { };

    public Func<Vector2Int, TileType?> tryGetTile = _ => null;
    public Func<Vector2Int, Unit> tryGetUnit = _ => null;

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

    public Vector2Int GetAxis(int axisIndex) {
        return (axisIndex == right ? cameraRig.transform.right : cameraRig.transform.forward).ToVector2().RoundToInt();
    }
    public int GetAxisValue(int axisIndex) {
        return Mathf.RoundToInt(Input.GetAxisRaw(axisNames[axisIndex]));
    }

    public IEnumerator RepeatLoop(int axisIndex) {

        if (TryGetPosition(out var startPosition) && TrySetPosition(startPosition + GetAxis(axisIndex) * GetAxisValue(axisIndex))) {

            var startTime = Time.time;
            yield return new WaitWhile(() => Time.time < startTime + repeatDelay && GetAxisValue(axisIndex) != 0);

            int value;
            while ((value = GetAxisValue(axisIndex)) != 0 && TryGetPosition(out var position) && TrySetPosition(position + GetAxis(axisIndex) * value)) {
                startTime = Time.time;
                var coolDown = repeatCoolDown / cameraRig.ToWorldUnits(1);
                yield return new WaitWhile(() => Time.time < startTime + coolDown && GetAxisValue(axisIndex) != 0);
            }
        }

        repeatLoops[axisIndex] = null;
    }


    public void Update() {

        if (enableMouse && cameraRig && cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition)) {
            if (!TryGetPosition(out var oldPosition) || oldPosition != mousePosition) {
                TrySetPosition(mousePosition);
                
            }
        }

        if (enableKeyboard) {

            var rightAxis = GetAxis(right);
            var upAxis = GetAxis(up);

            if (rightAxis.ManhattanLength() == 1 && upAxis.ManhattanLength() == 1 && rightAxis != upAxis) {
                if (repeatLoops[right] == null && GetAxisValue(right) != 0) {
                    repeatLoops[right] = RepeatLoop(right);
                    StartCoroutine(repeatLoops[right]);
                }
                if (repeatLoops[up] == null && GetAxisValue(up) != 0) {
                    repeatLoops[up] = RepeatLoop(up);
                    StartCoroutine(repeatLoops[up]);
                }
                if (viewRoot.activeSelf) {
                    Draw.ingame.Arrow(viewRoot.transform.position, viewRoot.transform.position + rightAxis.ToVector3() / 2, Vector3.up, .1f, Color.red);
                    Draw.ingame.Arrow(viewRoot.transform.position, viewRoot.transform.position + upAxis.ToVector3() / 2, Vector3.up, .1f, Color.blue);
                }
            }
        }

        if (viewRoot.activeSelf && TryGetPosition(out var position)) {
            viewRoot.transform.position = Vector3.Lerp(viewRoot.transform.position, position.ToVector3(),
                speed * Time.deltaTime / Vector2.Distance(viewRoot.transform.position.ToVector2(), position));
            if (text)
                text.text = position.ToString();
        }
    }
}