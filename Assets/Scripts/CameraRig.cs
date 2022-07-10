using System;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraRig : MonoBehaviour {

	private static CameraRig instance;
	public static CameraRig Instance {
		get {
			if (!instance) {
				instance = FindObjectOfType<CameraRig>();
				Assert.IsTrue(instance);
			}
			return instance;
		}
	}

	public LayerMask raycastLayerMask;
	public Transform arm;
	private Transform Arm {
		get {
			if (arm)
				return arm;
			arm = transform.Find("Arm");
			Assert.IsTrue(arm);
			return arm;
		}
	}

	public CinemachineVirtualCamera virtualCamera;
	private CinemachineVirtualCamera VirtualCamera {
		get {
			if (virtualCamera)
				return virtualCamera;
			virtualCamera = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
			Assert.IsTrue(virtualCamera);
			return virtualCamera;
		}
	}

	public float speed = 1.5f;
	public float speedMultiplier = 1;
	[NonSerialized] public Vector2 velocity;
	public float velocitySmoothTime = 0.05f;

	[NonSerialized] public float targetDistance = float.NaN;
	public float distance = 20;
	public float distanceSmoothTime = 50;
	public float distanceStep = -0.2f;
	public Vector2 distanceBounds = new(1, 30);

	public float rotationDuration = .3f;
	public Ease rotationEase = Ease.OutSine;
	public float rotationStep = -90;
	public float rotationAmplitude = 1.7f;
	public float rotationPeriod = 0;
	[NonSerialized] public float compassLastClickTime;

	public float pitchAngle = 40f;
	[NonSerialized] public float tagetPitchAngle = float.NaN;
	public float pitchAngleSmoothTime = .02f;
	public float pitchAngleSpeed = 90;
	public Vector2 pitchAngleBounds = new(0, 90);

	[NonSerialized] public bool isDragging;
	public Sequence rotationSequence;

	public float compassResetCooldown = .2f;

	[NonSerialized] public Vector3 oldMousePosition;

	[NonSerialized] public float lastClickTime;
	public float teleportCooldown = .2f;
	public float teleportDuration = .5f;
	public Ease teleportEase = Ease.OutExpo;
	public TweenerCore<Vector3, Vector3, VectorOptions> teleportAnimation;

	

	public void OnCompassClick() {
		if (rotationSequence == null) {
			RotateCameraRig(NextRotationAngle(1));
			compassLastClickTime = Time.unscaledTime;
		}
		else if (compassLastClickTime + compassResetCooldown > Time.unscaledTime) {
			rotationSequence.Kill();
			RotateCameraRig(0);
		}
	}

	private void Awake() {
		if (raycastLayerMask == 0)
			raycastLayerMask = 1 << LayerMask.NameToLayer("Default");
	}
	public void Jump(Vector3 position, bool canBeInterrupted=true) {
		teleportAnimation = transform.DOMove(position, teleportDuration).SetEase(teleportEase);
	}
	
	

	public void Update() {

		int sign(float value) => Mathf.Abs(value) < Mathf.Epsilon ? 0 : value > 0 ? 1 : -1;

		
		
		// WASD

		var input =
			transform.right.ToVector2() * sign(Input.GetAxisRaw("Horizontal")) +
			transform.forward.ToVector2() * sign(Input.GetAxisRaw("Vertical"));
		
		if (input != Vector2.zero) {
			velocity = input.normalized * (speed * distance);
			teleportAnimation?.Kill();
		}
		else
			velocity = Vector2.Lerp(velocity, Vector2.zero, velocitySmoothTime * Time.deltaTime); //Vector3.SmoothDamp(Velocity, TargetVelocity, ref Acceleration, VelocitySmoothTime);

		transform.position += Time.deltaTime * velocity.ToVector3();

		// CAMERA PITCH
		
		tagetPitchAngle = float.IsNaN(tagetPitchAngle)
			? pitchAngle
			: Mathf.Clamp(tagetPitchAngle + sign(Input.GetAxisRaw("PitchCamera")) * pitchAngleSpeed * Time.deltaTime,
				pitchAngleBounds[0], pitchAngleBounds[1]);
		pitchAngle = tagetPitchAngle;

		Arm.localRotation = Quaternion.Euler(pitchAngle, 0, 0);

		// QE ROTATION

		if (rotationSequence == null) {
			var rotate = sign(Input.GetAxisRaw("RotateCamera"));
			if (rotate != 0)
				RotateCameraRig(NextRotationAngle(rotate));
		}

		// ZOOM

		targetDistance = float.IsNaN(targetDistance)
			? distance
			: Mathf.Clamp(targetDistance + sign(Input.GetAxisRaw("Mouse ScrollWheel")) * distanceStep * distance,
				distanceBounds[0], distanceBounds[1]);
		distance =
			Mathf.Lerp(distance, targetDistance, Time.deltaTime * distanceSmoothTime);

		VirtualCamera.transform.localPosition = Vector3.back * distance;

		// DRAGGING

		if (Input.GetMouseButtonDown(2) && !isDragging) {
			isDragging = true;
			oldMousePosition = Input.mousePosition;
			if (teleportAnimation != null) {
				teleportAnimation.Kill();
				teleportAnimation = null;
			}
		}
		if (Input.GetMouseButtonUp(2) && isDragging)
			isDragging = false;

		if (isDragging) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			var rayOld = Camera.main.ScreenPointToRay(oldMousePosition);
			var plane = new Plane(Vector3.up, Vector3.zero);
			if (plane.Raycast(ray, out var enter) && plane.Raycast(rayOld, out var enterOld)) {
				var point = ray.GetPoint(enter);
				var pointOld = rayOld.GetPoint(enterOld);
				transform.position -= point - pointOld;
			}
			oldMousePosition = Input.mousePosition;
		}

		// TELEPORT

		if (Input.GetMouseButtonDown(2)) {
			if (lastClickTime + teleportCooldown > Time.unscaledTime) {
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				var plane = new Plane(Vector3.up, Vector3.zero);
				if (plane.Raycast(ray, out var enter)) 
					Jump(ray.GetPoint(enter));
			}
			else
				lastClickTime = Time.unscaledTime;
		}
	}

	public float NextRotationAngle(float axis) {
		var from = Mathf.RoundToInt(transform.rotation.eulerAngles.y / rotationStep) * rotationStep;
		var to = from + axis * rotationStep;
		return to;
	}
	public void RotateCameraRig(float angle) {
		rotationSequence = DOTween.Sequence()
			.Append(transform.DORotate(new Vector3(0, angle, 0), rotationDuration)
				.SetEase(rotationEase))
			.AppendCallback(() => rotationSequence = null);
	}
}