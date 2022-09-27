using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.PostProcessing;

public class BattleView : MonoBehaviour {

	public Camera camera;
	public float fadeDuration = 1;
	public Tweener fadeTweener;
	public ColorGrading colorGrading;
	public Ease fadeEase = default;

	public Rect cameraOffscreenRect = new Rect();
	public Rect cameraRect = new Rect();
	public Tweener cameraRectTweener;
	public float rectDuration = .5f;
	public Ease rectEase = Ease.OutExpo;

	public PostProcessProfile battleViewPostProcessProfile;

	public List<UnitView> unitViews = new List<UnitView>();

	public Dictionary<UnitView, List<UnitView>> targets;

	public Transform[] spawnPoints = Array.Empty<Transform>();

	public int Layer {
		set {
			gameObject.SetLayerRecursively(value);
			camera.cullingMask = value;
		}
	}

	public void AnimateCameraRect((Color from, Color to) fade, (Rect from, Rect to) rect) {

		if (colorGrading) {
			fadeTweener?.Kill();
			colorGrading.colorFilter.value = fade.from;
			fadeTweener = DOTween.To(
					() => colorGrading.colorFilter.value,
					value => colorGrading.colorFilter.value = value,
					fade.to,
					fadeDuration)
				.SetEase(fadeEase);
		}

		cameraRectTweener?.Kill();
		camera.rect = TransformRect(rect.from);
		cameraRectTweener = camera.DORect(TransformRect(rect.to), rectDuration).SetEase(rectEase, .01f);
	}

	public Rect TransformRect(Rect rect) {

		if (side <= 0)
			return rect;

		var flip = Matrix4x4.Translate(new Vector2(1, 0)) * Matrix4x4.Scale(new Vector2(-1, 1));
		var a = flip.MultiplyPoint(rect.min);
		var b = flip.MultiplyPoint(rect.max);
		var minX = Mathf.Min(a.x, b.x);
		var maxX = a.x + b.x - minX;
		var minY = Mathf.Min(a.y, b.y);
		var maxY = a.y + b.y - minY;
		rect = new Rect(minX, minY, maxX - minX, maxY - minY);

		return rect;
	}

	[ContextMenu(nameof(FindSpawnPoints))]
	public void FindSpawnPoints() {
		spawnPoints = GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("SpawnPoint")).ToArray();
	}

	public void Setup(UnitView unitViewPrefab, int count) {

		camera = GetComponentInChildren<Camera>();
		Assert.IsTrue(camera);

		if (battleViewPostProcessProfile)
			colorGrading = battleViewPostProcessProfile.GetSetting<ColorGrading>();

		Assert.IsTrue(count <= spawnPoints.Length);
		Assert.IsTrue(unitViewPrefab);

		for (var i = 0; i < count; i++) {
			var spawnPoint = spawnPoints[i];
			var unitView = Instantiate(unitViewPrefab, spawnPoint.position, spawnPoint.rotation, transform);
			unitView.gameObject.SetLayerRecursively(gameObject.layer);
			unitViews.Add(unitView);
		}
	}

	public void AssignTargets(IList<UnitView> targets) {

		Assert.AreNotEqual(0, unitViews.Count);
		Assert.AreNotEqual(0, targets.Count);

		this.targets = new Dictionary<UnitView, List<UnitView>>();
		foreach (var unitView in unitViews)
			this.targets.Add(unitView, new List<UnitView>());

		for (var i = 0; i < Mathf.Max(unitViews.Count, targets.Count); i++) {

			var a = unitViews[i % unitViews.Count];
			var b = targets[i % targets.Count];

			this.targets[a].Add(b);
		}

		foreach (var unitView in this.targets.Keys) {
			if (unitView.turret && unitView.turret.ballisticComputer && this.targets[unitView].Count > 0 && this.targets[unitView][0].center)
				unitView.turret.ballisticComputer.target = this.targets[unitView][0].center;
		}
	}

	public void OnDrawGizmos() {

		if (targets == null)
			return;

		Gizmos.color = Color.red;
		foreach (var attacker in targets.Keys) {
			if (attacker.center)
				foreach (var target in targets[attacker])
					if (target.center)
						Gizmos.DrawLine(attacker.center.position, target.center.position);
		}
	}


	[Range(-1, 1)] public int side = -1;

	public bool visible;

	public void Update() {

		if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
			if (!visible)
				AnimateCameraRect((Color.black, Color.white), (cameraOffscreenRect, cameraRect));
			else
				AnimateCameraRect((Color.white, Color.black), (cameraRect, cameraOffscreenRect));
			visible = !visible;
		}
	}
}