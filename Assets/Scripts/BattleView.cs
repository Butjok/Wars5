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
	public ParticleSystem impactParticleSystem;

	public PostProcessProfile battleViewPostProcessProfile;

	public List<UnitView> unitViews = new();
	public Dictionary<UnitView, List<ImpactPoint>> impactPoints = new();

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

		impactPoints.Clear();
		foreach (var unitView in unitViews)
			impactPoints.Add(unitView, new List<ImpactPoint>());

		for (var i = 0; i < Mathf.Max(unitViews.Count, targets.Count); i++) {

			var attacker = unitViews[i % unitViews.Count];
			var target = targets[i % targets.Count];

			Assert.AreNotEqual(0, target.impactPoints.Length);
			var impactPoint = target.impactPoints.Random();
			impactPoints[attacker].Add(impactPoint);
		}

		foreach (var attacker in impactPoints.Keys)
			if (attacker.turret && attacker.turret.ballisticComputer)
				attacker.turret.ballisticComputer.Target = impactPoints[attacker].Random().transform;
	}

	public int shooterIndex = -1;

	public bool Shoot() {
		shooterIndex = (shooterIndex + 1) % unitViews.Count;
		var shooter = unitViews[shooterIndex];
		shooter.turret.Fire(impactPoints[shooter]);
		return true;
	}

	[Range(-1, 1)] public int side = -1;

	public bool visible;

	public void Update() {

		if (Input.GetKeyDown(KeyCode.Alpha0) && side == -1) {
			Shoot();
		}

		if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
			if (!visible)
				AnimateCameraRect((Color.black, Color.white), (cameraOffscreenRect, cameraRect));
			else
				AnimateCameraRect((Color.white, Color.black), (cameraRect, cameraOffscreenRect));
			visible = !visible;
		}
	}
}