using System;
using DG.Tweening;
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

	public UnitView[] unitViews = Array.Empty<UnitView>();
	public BattleView other;
	
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

	public void Awake() {

		camera = GetComponentInChildren<Camera>();
		Assert.IsTrue(camera);

		if (battleViewPostProcessProfile)
			colorGrading = battleViewPostProcessProfile.GetSetting<ColorGrading>();

		unitViews = GetComponentsInChildren<UnitView>();

		if (other) {
			
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