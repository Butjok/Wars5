using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class TextFrame3d : MonoBehaviour {

    public float duration = .25f;
    public Ease ease = Ease.Unset;
    public float zOffset = .1f;
    public Camera camera;
    public LayerMask raycastLayerMask;

    public TextMeshPro campaignText;
    public TextMeshPro quitText;

    public void SetTarget(TextMeshPro target, float duration) {
        if (target == Target)
            return;
        Target = target;

        StopAllCoroutines();
        StartCoroutine(SelectionAnimation(target, duration));
    }
    
    public IEnumerator SelectionAnimation(TextMeshPro target, float duration) {

        var rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue(rectTransform);

        var size = target.GetPreferredValues();
        var startScale = rectTransform.localScale;
        var startPosition = rectTransform.anchoredPosition3D;
        var targetScale = new Vector3(size.x, size.y, 1);
        var targetPosition = target.rectTransform.anchoredPosition3D + target.rectTransform.forward * zOffset;

        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.OutExpo(t);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition3D = targetPosition;
    }
    public TextMeshPro Target { get; private set; }

    public TextMeshPro[] targets = { };

    private void OnEnable() {
        Assert.AreNotEqual(0, targets.Length);
        SetTarget(targets[0], 0);
    }

    public Vector3 lastMousePosition;

    private void Update() {

        foreach (var target in targets) {
            var meshCollider = target.GetComponent<MeshCollider>();
            if (meshCollider)
                meshCollider.sharedMesh = target.mesh;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            Assert.AreNotEqual(0, targets.Length);
            var index = Array.IndexOf(targets, Target);
            var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            var nextIndex = (index + offset).PositiveModulo(targets.Length);
            SetTarget(targets[nextIndex], duration);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
            Click(Target);
            return;
        }

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var hasHit = Physics.Raycast(ray, out var hit, float.MaxValue, raycastLayerMask);
        if (hasHit) {
            var textMeshPro = hit.collider.GetComponentInParent<TextMeshPro>();
            Assert.IsTrue(textMeshPro);
            if (Input.mousePosition != lastMousePosition && Target != textMeshPro)
                SetTarget(textMeshPro, duration);
            lastMousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(Mouse.left))
                Click(Target);
        }
    }

    public void Click(TextMeshPro text) {
        if (text == campaignText)
            MainMenuSelectionState.goToCampaign = true;
        else if (text == quitText)
            MainMenuSelectionState.quit = true;
    }
}