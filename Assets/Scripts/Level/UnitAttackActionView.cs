using System;
using Butjok.CommandLine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
public class UnitAttackActionView : MonoBehaviour {

    public static UnitAttackActionView Prefab => nameof(UnitAttackActionView).LoadAs<UnitAttackActionView>();

    public UnitAction action;
    public LineRenderer lineRenderer;
    public RectTransform uiRoot;
    public UIFrame uiFrame;
    public TMP_Text uiText;
    public string uiTextFormat = "−{0}";
    public float radius = .33f;

    private void Reset() {
        lineRenderer = GetComponent<LineRenderer>();
        Assert.IsTrue(lineRenderer);
        lineRenderer.enabled = false;
    }
    private void Update() {

        if (lineRenderer.enabled) {

            var source = action.unit.view.center.position;
            var destination = action.targetUnit.view.center.position;
            if (Vector3.Distance(source, destination) < 2 * radius) {
                lineRenderer.positionCount = 0;
                return;
            }

            var direction = (destination - source).normalized;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, source + direction * radius);
            lineRenderer.SetPosition(1, destination - direction * radius);
        }
    }

    [Command]
    public bool Show {
        set {

            action.unit.view.turret.aim = value;
            uiRoot.gameObject.SetActive(value);
            lineRenderer.enabled = value;
            action.targetUnit.view.HighlightAsTarget = value;

            if (!value)
                return;

            action.unit.view.turret.computer.Target = action.targetUnit.view.body.transform;

            uiFrame.boxCollider = action.targetUnit.view.uiBoxCollider;

            var (_, newTargetHp) = action.CalculateHpsAfterAttack();
            var difference = action.targetUnit.hp.v - newTargetHp;
            uiText.text = string.Format(uiTextFormat, difference);
        }
    }
}