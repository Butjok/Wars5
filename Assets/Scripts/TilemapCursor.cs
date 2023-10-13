using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using Butjok.CommandLine;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    public GameObject uiRoot;
    public GameObject tileRoot;
    public Image tileThumbnail;
    public TMP_Text tileName;
    public GameObject unitRoot;
    public Image unitThumbnail;
    public TMP_Text unitHp;
    public TMP_Text unitName;

    public Sprite[] tileThumbnails = { };
    public Sprite[] unitThumbnails = { };

    public bool ShowUi {
        set {
            if (uiRoot.activeSelf != value)
                uiRoot.SetActive(value);
        }
    }
    public bool ShowTile {
        set {
            if (tileRoot.activeSelf != value)
                tileRoot.SetActive(value);
        }
    }
    public bool ShowUnit {
        set {
            if (unitRoot.activeSelf != value)
                unitRoot.SetActive(value);
        }
    }
    public TileType TileType {
        set {
            tileThumbnail.sprite = tileThumbnails.FirstOrDefault(image => image.name == value.ToString());
            tileName.text = TileInfos.GetName(value);
        }
    }
    public Building Building {
        set {
            tileThumbnail.sprite = tileThumbnails.FirstOrDefault(image => image.name == $"{value.Player.coName}{value.type}");
            tileName.text = TileInfos.GetName(value);
        }
    }
    public Unit Unit {
        set {
            unitThumbnail.sprite = unitThumbnails.FirstOrDefault(image => image.name == $"{value.Player.coName}{value.type}");
            unitName.text = UnitInfo.GetShortName(value);
            if (value.Hp == Rules.MaxHp(value))
                unitHp.enabled = false;
            else {
                unitHp.enabled = true;
                unitHp.text = value.Hp.ToString();
            }
        }
    }

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

        position = value;
        if (!viewRoot.activeSelf)
            viewRoot.SetActive(true);
        if (text && text!.enabled)
            text.enabled = viewRoot.activeSelf;

        return true;
    }
    public void Hide() {
        viewRoot.SetActive(false);
        ShowUi = false;
    }

    public void Set(Vector2Int position, TileType tileType, Building building, Unit unit) {

        TrySetPosition(position);

        ShowUi = true;

        if (building != null) {
            ShowTile = true;
            Building = building;
        }
        else if (tileType != TileType.None) {
            ShowTile = true;
            TileType = tileType;
        }
        else
            ShowTile = false;

        if (unit != null) {
            ShowUnit = true;
            Unit = unit;
        }
        else
            ShowUnit = false;
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

    public bool hideOnStart = true;

    public void Awake() {
        if (hideOnStart)
            Hide();
    }

    public void Update() {

        /*if (enableMouse && cameraRig && cameraRig.camera.TryGetMousePosition(out Vector2Int mousePosition)) {
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
        }*/

        if (viewRoot.activeSelf && TryGetPosition(out var position)) {
            viewRoot.transform.position = Vector3.Lerp(viewRoot.transform.position, position.ToVector3(),
                speed * Time.deltaTime / Vector2.Distance(viewRoot.transform.position.ToVector2(), position));
            if (text)
                text.text = position.ToString();
        }
    }

    private void OnGUI() {
        GUI.skin = DefaultGuiSkin.TryGet;
        if (viewRoot.activeSelf && TryGetPosition(out var position)) {
            var content = new GUIContent(position.ToString());
            var size = GUI.skin.label.CalcSize(content);
            GUI.Label(new Rect(0, Screen.height - size.y, size.x, size.y), content);
        }
    }
}