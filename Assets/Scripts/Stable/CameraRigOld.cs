using System;
using System.Collections;
using Butjok.CommandLine;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraRigOld : MonoBehaviour {

    private static CameraRigOld instance;
    public static bool TryFind(out CameraRigOld result) {
        if (!instance)
            instance = FindObjectOfType<CameraRigOld>();
        result = instance;
        return result;
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
    public Vector2 speedRange = new Vector2(1, 10);
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
    public bool clampRotation = true;
    public Vector2 rotationRange = new(-45, 45);
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
    public Tween teleportAnimation;

    public PlaceOnTerrain placeOnTerrain;
    [Command]
    public bool PlaceOnTerrain {
        get => placeOnTerrain && placeOnTerrain.enabled;
        set {
            if (placeOnTerrain)
                placeOnTerrain.enabled = value;
        }
    }

    public Easing.Name rotationEasingName = Easing.Name.OutSine;

    public float speedupMultiplier = 2;
    public Camera camera;

    public void OnCompassClick() {
        if (rotationSequence == null) {
            TryRotate(1);
            compassLastClickTime = Time.unscaledTime;
        }
        else if (compassLastClickTime + compassResetCooldown > Time.unscaledTime) {
            rotationSequence.Kill();
            TryRotate(0);
        }
    }

    private bool initialized;
    private void EnsureInitialized() {
        if (initialized)
            return;
        initialized = true;

        placeOnTerrain = GetComponent<PlaceOnTerrain>();

        if (raycastLayerMask == 0)
            raycastLayerMask = 1 << LayerMask.NameToLayer("Default");
    }

    private void Awake() {
        EnsureInitialized();
    }

    public Tween Jump(Vector3 targetPosition) {
        teleportAnimation?.Kill();
        placeOnTerrain.enabled = false;
        teleportAnimation = transform.DOMove(targetPosition, teleportDuration).SetEase(teleportEase);
        teleportAnimation.timeScale = 1;
        teleportAnimation.onComplete += () => placeOnTerrain.enabled = true;
        teleportAnimation.onKill += teleportAnimation.onComplete;
        return teleportAnimation;
    }

    public Vector2 fovBounds = new Vector2(45, 30);
    public float fovLerpPower = 1;

    public float speedInViewportSpace = .2f;

    private void Update() {

        int Sign(float value) => Mathf.Abs(value) < Mathf.Epsilon ? 0 : value > 0 ? 1 : -1;


        // WASD

        var input =
            transform.right.ToVector2() * Sign(Input.GetAxisRaw("Horizontal")) +
            transform.forward.ToVector2() * Sign(Input.GetAxisRaw("Vertical"));

        if (input != Vector2.zero) {
            
            Assert.IsTrue(camera);
            var ray0 = camera.ScreenPointToRay(new Vector3(0, 0));
            var ray1 = camera.ScreenPointToRay(new Vector3(1, 0));
            var plane = new Plane(Vector3.up, transform.position);
            if (plane.Raycast(ray0, out var enter0) && plane.Raycast(ray1, out var enter1)) {
                var point0 = ray0.GetPoint(enter0);
                var point1 = ray1.GetPoint(enter1);
                var rr = Vector3.Distance(point0, point1);
                var onePixelInWorldUnits = Mathf.Sqrt(rr / 2);
                var speedInPixels = Screen.width * speedInViewportSpace;
                velocity = input.normalized * onePixelInWorldUnits * speedInPixels * (Input.GetKey(KeyCode.LeftShift) ? speedupMultiplier : 1);
            }
            
//            velocity = input.normalized * (speed * distance) * (Input.GetKey(KeyCode.LeftShift) ? speedupMultiplier : 1);
            teleportAnimation?.Kill();
        }
        else
            velocity = Vector2.Lerp(velocity, Vector2.zero, velocitySmoothTime * Time.unscaledDeltaTime); //Vector3.SmoothDamp(Velocity, TargetVelocity, ref Acceleration, VelocitySmoothTime);

        transform.position += Time.unscaledDeltaTime * velocity.ToVector3();

        // CAMERA PITCH

        tagetPitchAngle = float.IsNaN(tagetPitchAngle)
            ? pitchAngle
            : Mathf.Clamp(tagetPitchAngle + Sign(Input.GetAxisRaw("PitchCamera")) * pitchAngleSpeed * Time.unscaledDeltaTime,
                pitchAngleBounds[0], pitchAngleBounds[1]);
        pitchAngle = tagetPitchAngle;

        Arm.localRotation = Quaternion.Euler(pitchAngle, 0, 0);

        // QE ROTATION

        if (rotationCoroutine == null) {
            var rotationDirection = Sign(Input.GetAxisRaw("RotateCamera"));
            if (rotationDirection != 0)
                TryRotate(direction: rotationDirection);
        }

        // ZOOM

        targetDistance = float.IsNaN(targetDistance)
            ? distance
            : Mathf.Clamp(targetDistance + Sign(Input.GetAxisRaw("Mouse ScrollWheel")) * distanceStep * distance,
                distanceBounds[0], distanceBounds[1]);
        distance =
            Mathf.Lerp(distance, targetDistance, Time.unscaledDeltaTime * distanceSmoothTime);

        VirtualCamera.transform.localPosition = Vector3.back * distance;
        var t = (distance - distanceBounds[0]) / (distanceBounds[1] - distanceBounds[0]);
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(fovBounds[0], fovBounds[1], Mathf.Pow(t, fovLerpPower));

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

        if (isDragging && Camera.main) {
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
            if (lastClickTime + teleportCooldown > Time.unscaledTime && Mouse.TryGetPosition(out Vector3 target)) {
                Jump(target);
            }
            else
                lastClickTime = Time.unscaledTime;
        }
    }

    public IEnumerator rotationCoroutine;

    public bool TryRotate(int? direction = null, float? angle = null) {

        Assert.IsFalse(direction == null && angle == null ||
                       direction != null && angle != null);

        if (rotationCoroutine != null) {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        var startAngle = transform.eulerAngles.y;

        var targetAngle = 0f;
        if (direction is { } actualDirection)
            targetAngle = (Mathf.Round(startAngle / rotationStep) + actualDirection) * rotationStep;
        if (angle is { } actualAngle)
            targetAngle = actualAngle;

        while (targetAngle > 180)
            targetAngle -= 360;
        while (targetAngle < -180)
            targetAngle += 360;

        if (clampRotation)
            targetAngle = Mathf.Clamp(targetAngle, rotationRange[0], rotationRange[1]);

        rotationCoroutine = RotationAnimation(startAngle, targetAngle);
        StartCoroutine(rotationCoroutine);
        return true;
    }

    public IEnumerator RotationAnimation(float startAngle, float targetAngle) {

        var from = Quaternion.Euler(0, startAngle, 0);
        var to = Quaternion.Euler(0, targetAngle, 0);
        if (from == to) {
            rotationCoroutine = null;
            yield break;
        }

        var startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + rotationDuration) {
            var t = (Time.unscaledTime - startTime) / rotationDuration;
            t = Easing.Dynamic(rotationEasingName, t);
            transform.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        transform.rotation = to;
        rotationCoroutine = null;
    }

    [Command]
    public float Fov {
        get => virtualCamera.m_Lens.FieldOfView;
        set => virtualCamera.m_Lens.FieldOfView = value;
    }
}

public interface ITween {
     bool IsCompleted { get; }
     void Kill();
     void Complete();
}