using System;
using System.Collections;
using System.Linq;
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
    public TextMeshPro aboutText;
    public TextMeshPro settingsText;
    public TextMeshPro loadGameText;
    
    public Vector2 margin = new Vector2(.5f, .1f);

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
        
        var renderer = GetComponent<Renderer>();
        Assert.IsTrue(renderer);

        var sizeUniformName = Shader.PropertyToID("_Size");
        void UpdateUniform() {
            renderer.sharedMaterial.SetVector(sizeUniformName, new Vector4(rectTransform.localScale.x, rectTransform.localScale.y, 1, 1));
        }

        var size = target.GetPreferredValues();
        var startScale = rectTransform.localScale;
        var startPosition = rectTransform.anchoredPosition3D;
        var targetScale = new Vector3(size.x + margin.x * 2, size.y + margin.y * 2, 1);
        var targetPosition = target.rectTransform.anchoredPosition3D + target.rectTransform.forward * zOffset;

        var startTime = Time.time;
        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            t = Easing.OutExpo(t);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, t);
            UpdateUniform();
            yield return null;
        }

        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition3D = targetPosition;
        UpdateUniform();
    }
    public TextMeshPro Target { get; private set; }

    public TextMeshPro[] targets = { };

    private void OnEnable() {
        Assert.AreNotEqual(0, targets.Length);
        SetTarget(targets[0], 0);
        lastMousePosition = null;
    }

    public Vector3? lastMousePosition;

    private void Update() {

        if (InputState.TryConsumeKeyDown(KeyCode.Tab)) {
            var activeTargets = targets.Where(t => t.color == Color.white).ToList();
            Assert.AreNotEqual(0, activeTargets.Count);
            var index = activeTargets.IndexOf(Target);
            var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
            var nextIndex = (index + offset).PositiveModulo(activeTargets.Count);
            SetTarget(activeTargets[nextIndex], duration);
            return;
        }

        if (InputState.TryConsumeKeyDown(KeyCode.Return) || InputState.TryConsumeKeyDown(KeyCode.Space)) {
            Click(Target);
            return;
        }

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        foreach (var target in targets) {
            
            if (target.color != Color.white)
                continue;

            var meshCollider = target.GetComponentInChildren<MeshCollider>();
            Assert.IsTrue(meshCollider);

            if (meshCollider.sharedMesh != target.mesh) {
                meshCollider.transform.position = target.transform.position;
                meshCollider.sharedMesh = target.mesh;
            }

            if (!meshCollider.Raycast(ray, out _, float.MaxValue))
                continue;
                
            if (Input.mousePosition != lastMousePosition && Target != target )
                SetTarget(target, duration);
            lastMousePosition = Input.mousePosition;

            if (InputState.TryConsumeMouseButtonDown(Mouse.left))
                Click(Target);
            break;
        }
    }

    public void Click(TextMeshPro text) {
        if (text == campaignText)
            MainMenuSelectionState.goToCampaign = true;
        else if (text == quitText)
            MainMenuSelectionState.quit = true;
        else if (text == aboutText)
            MainMenuSelectionState.goToAbout = true;
        else if (text == settingsText)
            MainMenuSelectionState.goToSettings = true;
        else if (text == loadGameText)
            MainMenuSelectionState.goToLoadGame = true;
    }
}