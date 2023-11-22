using System;
using System.Collections;
using Butjok.CommandLine;
using Cinemachine;
using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class CameraRig : MonoBehaviour {

    public Camera camera;
    public CinemachineVirtualCamera virtualCamera;

    [Header("WASD")]
    public float speedInViewportSpace = .15f;
    public float speedUpMultiplier = 2;
    public float velocityDecelerationFactor = 25;

    [Header("Pitch")]
    public Transform arm;
    public float pitchAngleSpeed = 135;
    public Vector2 pitchClamp = new(10, 80);

    [Header("Rotation")]
    public float rotationDuration = .15f;
    public Easing.Name rotationEasing = Easing.Name.OutSine;
    public float rotationStep = 45;
    public bool clampYaw = true;
    public Vector2 yawRange = new(-45, 45);

    [Header("Dragging")]
    public float maxDragLengthInViewportSpace = .1f;

    [Header("Jump")]
    public float jumpCooldown = .2f;
    public float jumpDuration = .5f;
    public Easing.Name jumpEasing = Easing.Name.OutExpo;

    [Header("Zoom")]
    public Vector2 distanceClamp = new(2, 40);
    public float distanceStep = -.2f;
    public float distanceStepDuration = .1f;
    public Easing.Name zoomEasing = Easing.Name.OutExpo;
    public float zoomLerpFactor = 50;
    public Vector2 zoomCursorFactor = new(.5f, .5f);
    public float zoomSpeed = 1;

    [Header("Free look")]
    public bool enableFreeLook = false;
    public Vector2 freeLookSpeed = new(180, 180);

    [Header("Fov")]
    public Vector2 fov = new(45, 30);

    private Vector2 velocity;
    private float distanceDelta;
    private float lastClickTime = float.MinValue;
    public float targetDistance;

    public float targetWidth = 1;
    public float widthSpeed = .5f;

    public bool acceptInput = true;

    public float ToWorldUnits(float sizeInViewportSpace) {
        var ray0 = camera.ScreenPointToRay(new Vector3(0, 0));
        var ray1 = camera.ScreenPointToRay(new Vector3(1, 0));
        var plane = new Plane(Vector3.up, transform.position);
        float worldUnitsPerPixel = 0;
        if (plane.Raycast(ray0, out var enter0) && plane.Raycast(ray1, out var enter1)) {
            var point0 = ray0.GetPoint(enter0);
            var point1 = ray1.GetPoint(enter1);
            var rr = Vector3.Distance(point0, point1);
            worldUnitsPerPixel = Mathf.Sqrt(rr / 2);
        }
        var sizeInPixels = Screen.width * sizeInViewportSpace;
        return sizeInPixels * worldUnitsPerPixel;
    }

    public float DollyZoom {
        get => dollyZoom;
        set {
            targetDollyZoom = dollyZoom = value;
            Fov = Mathf.Lerp(dollyZoomFovRange[0], dollyZoomFovRange[1], dollyZoom);
            Distance = Mathf.Lerp(dollyZoomWidthRange[0], dollyZoomWidthRange[1], dollyZoom) / (2 * Mathf.Tan(Mathf.Deg2Rad * Fov / 2));
        }
    }

    public Vector2 dollyZoomFovRange = new(45, 10);
     public Vector2 dollyZoomWidthRange = new(2.5f, 15);
    [Range(0, 1)] [SerializeField] private float dollyZoom = 0;
    [Range(0, 1)] public float targetDollyZoom = 0;
    public float dollyZoomSpeed = 20;

    [Command]
    public static float verticalStretch = 1.05f;

    public Bounds? bounds;

    private void Awake() {
        var fov = Mathf.Lerp(dollyZoomFovRange[0], dollyZoomFovRange[1], dollyZoom);
        var width = Mathf.Lerp(dollyZoomWidthRange[0], dollyZoomWidthRange[1], dollyZoom);
        Fov = fov;
        Distance = width / (2 * Mathf.Tan(Mathf.Deg2Rad * fov / 2));
    }

    private void LateUpdate() {

        Assert.IsTrue(camera);
        Assert.IsTrue(arm);


        var positionInput =
            transform.right.ToVector2() * Input.GetAxisRaw("Horizontal").ZeroSign() +
            transform.forward.ToVector2() * Input.GetAxisRaw("Vertical").ZeroSign();

        if (positionInput != Vector2.zero) {
            StopJump();
            velocity = positionInput.normalized * ToWorldUnits(speedInViewportSpace) * (Input.GetKey(KeyCode.LeftShift) ? speedUpMultiplier : 1);
        }
        else
            velocity = Vector2.Lerp(velocity, Vector2.zero, velocityDecelerationFactor * Time.unscaledDeltaTime);

        transform.position += velocity.ToVector3() * Time.unscaledDeltaTime;

        var pitchInput = Input.GetAxisRaw("PitchCamera").ZeroSign();
        if (pitchInput != 0)
            PitchAngle += pitchInput * pitchAngleSpeed * Time.unscaledDeltaTime;

        var rotationDirection = Input.GetAxisRaw("RotateCamera").ZeroSign();
        if (rotationCoroutine == null && rotationDirection != 0)
            TryRotate(rotationDirection);

        // ZOOM

        targetDollyZoom = Mathf.Clamp01(targetDollyZoom + Input.GetAxisRaw("Mouse ScrollWheel").ZeroSign() * distanceStep);
        dollyZoom = Mathf.Lerp(dollyZoom, targetDollyZoom, Time.unscaledDeltaTime * dollyZoomSpeed);
        var fov = Mathf.Lerp(dollyZoomFovRange[0], dollyZoomFovRange[1], dollyZoom);
        var width = Mathf.Lerp(dollyZoomWidthRange[0], dollyZoomWidthRange[1], dollyZoom);

        var oldDistance = Distance;
        var plane = new Plane(Vector3.up, Vector3.zero);

        var oldRay = camera.FixedScreenPointToRay(Input.mousePosition);

        var oldRaycast = plane.Raycast(oldRay, out var oldEnter);
        Fov = fov;
        Distance = width / (2 * Mathf.Tan(Mathf.Deg2Rad * fov / 2));
        var delta = Distance - oldDistance;

        var newRay = camera.FixedScreenPointToRay(Input.mousePosition);

        var newRaycast = plane.Raycast(newRay, out var newEnter);
        if (oldRaycast && newRaycast) {
            var oldPoint = oldRay.GetPoint(oldEnter);
            var newPoint = newRay.GetPoint(newEnter);
            var cursorFactor = delta < 0 ? zoomCursorFactor[0] : zoomCursorFactor[1];
            transform.position -= cursorFactor * (newPoint - oldPoint);
        }


        /*var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, transform.position).Raycast(ray, out var enter)) {
            var point = ray.GetPoint(enter);
            transform.position -= zoomMouseFactor * cursorFactor * (point - transform.position) * delta / oldDistance;
        }*/

        if (enableFreeLook) {
            if (freeLookCoroutine == null && Input.GetKeyDown(KeyCode.LeftAlt)) {
                freeLookCoroutine = FreeLookCoroutine();
                StartCoroutine(freeLookCoroutine);
            }
            if (freeLookCoroutine != null && Input.GetKeyUp(KeyCode.LeftAlt)) {
                StopCoroutine(freeLookCoroutine);
                freeLookCoroutine = null;
            }
        }

        if (draggingCoroutine == null && Input.GetMouseButtonDown(Mouse.middle)) {
            draggingCoroutine = DraggingAnimation();
            StartCoroutine(draggingCoroutine);
            Cursor.visible = false;
        }
        if (draggingCoroutine != null && Input.GetMouseButtonUp(Mouse.middle)) {
            StopCoroutine(draggingCoroutine);
            draggingCoroutine = null;
            Cursor.visible = true;
        }

        // JUMP

        if (Input.GetMouseButtonDown(Mouse.middle)) {
            if (lastClickTime + jumpCooldown > Time.unscaledTime && camera.TryGetMousePosition(out Vector3 target))
                Jump(target.ToVector2().ToVector3());
            else
                lastClickTime = Time.unscaledTime;
        }
        
        // clamp

        if (bounds is { } actualBounds) {
            var closestPoint = actualBounds.ClosestPoint(transform.position);
            closestPoint.y  = transform.position.y;
            transform.position = closestPoint;
        }
    }
    private Vector3 oldMousePosition;

    private IEnumerator freeLookCoroutine;
    private IEnumerator FreeLookCoroutine() {

        Vector2 GetMousePositionInViewport() {
            var position = Input.mousePosition;
            position.x /= Screen.width;
            position.y /= Screen.height;
            return position;
        }

        // Cursor.lockState = CursorLockMode.Locked;
        // yield return null;
        // Cursor.lockState = CursorLockMode.Confined;

        var oldMousePosition = GetMousePositionInViewport();
        while (true) {
            yield return null;
            var mousePosition = GetMousePositionInViewport();
            var delta = mousePosition - oldMousePosition;
            var pitchInput = delta.y;
            if (!Mathf.Approximately(0, pitchInput))
                PitchAngle += pitchInput * freeLookSpeed.x;
            var rotationInput = delta.x;
            if (!Mathf.Approximately(0, rotationInput)) {
                StopRotation();
                Yaw += rotationInput * freeLookSpeed.y;
            }
            oldMousePosition = mousePosition;
        }
    }

    private IEnumerator draggingCoroutine;
    private IEnumerator DraggingAnimation() {
        var plane = new Plane(Vector3.up, transform.position);
        var oldMousePosition = Input.mousePosition;
        while (true) {
            if (Input.mousePosition != oldMousePosition) {
                var newRay = camera.ScreenPointToRay(Input.mousePosition);
                var oldRay = camera.ScreenPointToRay(oldMousePosition);
                if (!plane.Raycast(newRay, out var newEnter) || !plane.Raycast(oldRay, out var oldEnter))
                    continue;
                var newPoint = newRay.GetPoint(newEnter);
                var oldPoint = oldRay.GetPoint(oldEnter);
                //Draw.ingame.Line(oldPoint, newPoint, Color.cyan);
                var delta = oldPoint - newPoint;
                if (delta != Vector3.zero)
                    delta = delta.normalized * Mathf.Clamp(delta.magnitude, 0, ToWorldUnits(maxDragLengthInViewportSpace));
                transform.position += delta;
            }
            oldMousePosition = Input.mousePosition;
            yield return null;
        }
    }

    public IEnumerator JumpCoroutine { get; private set; }
    public void StopJump() {
        if (JumpCoroutine != null) {
            StopCoroutine(JumpCoroutine);
            JumpCoroutine = null;
        }
    }
    public void Jump(Vector3 to) {
        StopJump();
        var completed = false;
        JumpCoroutine = JumpAnimation(to, () => completed=true);
        StartCoroutine(JumpCoroutine);
    }
    private IEnumerator JumpAnimation(Vector3 to, Action onComplete) {
        var from = transform.position;
        var startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + jumpDuration) {
            var t = (Time.unscaledTime - startTime) / jumpDuration;
            t = Easing.Dynamic(jumpEasing, t);
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
        JumpCoroutine = null;
        onComplete?.Invoke();
    }

    public float PitchAngle {
        get => arm.localRotation.eulerAngles.x;
        set {
            var angles = arm.localRotation.eulerAngles;
            angles.x = Mathf.Clamp(value, pitchClamp[0], pitchClamp[1]);
            arm.localRotation = Quaternion.Euler(angles);
        }
    }
    public float Yaw {
        get => transform.rotation.eulerAngles.y;
        set {
            var angles = transform.rotation.eulerAngles;
            angles.y = ClampedYaw(value);
            transform.rotation = Quaternion.Euler(angles);
        }
    }
    public float Distance {
        get => -virtualCamera.transform.localPosition.z;
        private set {
            value = ClampedDistance(value);
            var position = virtualCamera.transform.localPosition;
            position.z = -value;
            virtualCamera.transform.localPosition = position;
            var t = (value - distanceClamp[0]) / (distanceClamp[1] - distanceClamp[0]);
            // Fov = Mathf.Lerp(fov[0], fov[1], Easing.Dynamic(fovEasing, t));
        }
    }

    public Easing.Name fovEasing = Easing.Name.Linear;

    public void SetDistance(float value, bool animate = true) {
        targetDistance = ClampedDistance(value);
        if (!animate)
            Distance = targetDistance;
    }
    public float Fov {
        get => camera.fieldOfView;
        set => camera.fieldOfView = value;
    }

    private float ClampedYaw(float yaw) {
        while (yaw > 180)
            yaw -= 360;
        while (yaw < -180)
            yaw += 360;
        if (clampYaw)
            yaw = Mathf.Clamp(yaw, yawRange[0], yawRange[1]);
        return yaw;
    }
    private float ClampedDistance(float distance) {
        return distance;
        //return Mathf.Clamp(distance, distanceClamp[0], distanceClamp[1]);
    }

    private IEnumerator rotationCoroutine;

    public bool StopRotation() {
        if (rotationCoroutine != null) {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
        return true;
    }

    public bool TryRotate(int? direction = null, float? angle = null) {

        Assert.IsFalse(direction == null && angle == null ||
                       direction != null && angle != null);

        StopRotation();

        var startYaw = transform.eulerAngles.y;

        var endYaw = 0f;
        if (direction is { } actualDirection)
            endYaw = (Mathf.Round(startYaw / rotationStep) + actualDirection) * rotationStep;
        if (angle is { } actualAngle)
            endYaw = actualAngle;

        endYaw = ClampedYaw(endYaw);

        //if (direction is {} actualDirection2 && Mathf.Abs((endYaw + 360) % 90 - 45) < 5)
        //  endYaw = ClampedYaw(endYaw + actualDirection2 * rotationStep);

        rotationCoroutine = RotationAnimation(startYaw, endYaw);
        StartCoroutine(rotationCoroutine);
        return true;
    }

    private IEnumerator RotationAnimation(float startYaw, float endYaw) {

        var from = Quaternion.Euler(0, startYaw, 0);
        var to = Quaternion.Euler(0, endYaw, 0);
        if (from == to) {
            rotationCoroutine = null;
            yield break;
        }

        var startTime = Time.unscaledTime;
        var actualDuration = Mathf.Abs(Quaternion.Angle(from, to) / rotationStep * rotationDuration);
        while (Time.unscaledTime < startTime + actualDuration) {
            var t = (Time.unscaledTime - startTime) / actualDuration;
            t = Easing.Dynamic(rotationEasing, t);
            transform.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        transform.rotation = to;
        rotationCoroutine = null;
    }
}