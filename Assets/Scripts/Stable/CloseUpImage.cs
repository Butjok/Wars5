using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CloseUpImage : MonoBehaviour, IDragHandler, IEndDragHandler {

    public Transform cameraArm;
    public Vector2 rotationSpeed = new(1, 1);
    public Vector2 pitchBounds = new(0,90);
    public Vector2 deceleration = new(1000,1000);
    public Vector2 startAngles;
    
    public Vector2 Angles {
        get =>  cameraArm.rotation.eulerAngles;
        set {
            value.x = Mathf.Clamp(value.x, pitchBounds[0], pitchBounds[1]);
            cameraArm.rotation = Quaternion.Euler(value);    
        }
    }
    public static Vector2 ToAngles(Vector2 mouseDeltaPosition) {
        return new Vector2(-mouseDeltaPosition.y, mouseDeltaPosition.x);
    }

    private void Awake() {
        startAngles = Angles;
    }

    private void OnEnable() {
        Angles = startAngles;
        speed=Vector2.zero;
    }

    public Vector2 speed;
    private void Update() {
        Angles += speed * Time.deltaTime;

        var newSpeed = speed - speed.normalized * deceleration * Time.deltaTime;
        if (speed.x * newSpeed.x < 0)
            newSpeed.x = 0;
        if (speed.y * newSpeed.y < 0)
            newSpeed.y = 0;
        speed = newSpeed;
    }

    public void OnDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        speed = Vector2.zero;
        Angles += ToAngles(eventData.delta) * rotationSpeed;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        speed = ToAngles(eventData.delta) * rotationSpeed / Time.deltaTime;
    }
}