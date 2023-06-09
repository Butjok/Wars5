using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PreselectionCursor : MonoBehaviour {

    private static PreselectionCursor instance;
    public static bool TryFind(out PreselectionCursor result) {
        if (!instance)
            instance = FindObjectOfType<PreselectionCursor>(true);
        result = instance;
        return result;
    }
    public static PreselectionCursor TryFind() {
        TryFind(out var preselectionCursor);
        return preselectionCursor;
    }

    public RectTransform root;
    public RectTransform arrow;
    public Vector2 margin;
    public bool diagonal = false;
    public Image thumbnail;
    public CameraRig cameraRig;

    public void ShowAt(Vector3 position, Sprite thumbnail = null) {
        transform.position = position;
        gameObject.SetActive(true);
        root.gameObject.SetActive(true);
        this.thumbnail.sprite = thumbnail;
    }
    public void Hide() {
        gameObject.SetActive(false);
        root.gameObject.SetActive(false);
    }
    public bool Visible => gameObject.activeSelf;
    private void LateUpdate() {

        root.gameObject.SetActive(false);

        var camera = Camera.main;
        if (!camera)
            return;

        Vector2 screenPosition = camera.WorldToScreenPoint(transform.position);
        var screenSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
        var screenSizeWithoutMargin = screenSize - margin * 2;
        var normalizedPosition = (screenPosition - screenSize / 2) / (screenSizeWithoutMargin / 2);
        if (new Rect(-1, -1, 2, 2).Contains(normalizedPosition))
            return;

        root.gameObject.SetActive(true);

        var clippedNormalizedPosition = diagonal
            ? normalizedPosition / Mathf.Max(Mathf.Abs(normalizedPosition.x), Mathf.Abs(normalizedPosition.y))
            : new Vector2(Mathf.Clamp(normalizedPosition.x, -1, 1), Mathf.Clamp(normalizedPosition.y, -1, 1));

        var clippedScreenPosition = (clippedNormalizedPosition * (screenSizeWithoutMargin / 2)) + screenSize / 2;
        root.anchoredPosition = clippedScreenPosition;

        var lookDirection = screenPosition - clippedScreenPosition;
        var angle = Vector2.SignedAngle(Vector2.up, lookDirection);
        arrow.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void JumpToTarget() {
        cameraRig.Jump(transform.position);
    }

    public bool VisibleOnTheScreen(Camera camera, Vector3 worldPosition) {
        Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
        var screenSize = new Vector2(camera.pixelWidth, camera.pixelHeight);
        var screenSizeWithoutMargin = screenSize - margin * 2;
        var normalizedPosition = (screenPosition - screenSize / 2) / (screenSizeWithoutMargin / 2);
        return new Rect(-1, -1, 2, 2).Contains(normalizedPosition);
    }
}