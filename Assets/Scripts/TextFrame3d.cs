using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class TextFrame3d : MonoBehaviour {

	public float duration = .25f;
	public Ease ease = Ease.Unset;
	public float zOffset = .1f;

	private Tween positionTween, scalingTween;

	private TextMeshPro target;
	public void SetTarget(TextMeshPro target, float duration) {
		if (target == this.target)
			return;
		this.target = target;

		positionTween?.Kill();
		scalingTween?.Kill();
		positionTween = null;
		scalingTween = null;

		var rectTransform = GetComponent<RectTransform>();
		Assert.IsTrue(rectTransform);

		var width = target.preferredWidth;
		var height = target.preferredHeight;

		scalingTween = rectTransform.DOScale(new Vector3(width, height, 1), duration).SetEase(ease);
		positionTween = rectTransform.DOAnchorPos3D(target.rectTransform.anchoredPosition3D + target.rectTransform.forward * zOffset, duration).SetEase(ease);
	}

	public TextMeshPro[] targets = { };

	private void Start() {
		Assert.AreNotEqual(0, targets.Length);
		SetTarget(targets[0], 0);
	}

	private void Update() {
		if (InputState.TryConsumeKeyDown(KeyCode.Tab)) {
			Assert.AreNotEqual(0, targets.Length);
			var index = Array.IndexOf(targets, target);
			var offset = Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
			var nextIndex = (index + offset).PositiveModulo(targets.Length);
			SetTarget(targets[nextIndex], duration);
		}
	}
}