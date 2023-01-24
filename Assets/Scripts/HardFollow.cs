using System;
using UnityEngine;

public class HardFollow : MonoBehaviour {

    public UnitView target;
    public bool selectRandomTarget = false;
    public ManualControl manualControl;

    public void Awake() {
        if (selectRandomTarget) 
            CycleTarget();
    }

    public void CycleTarget() {
        var views = FindObjectsOfType<UnitView>();
        if (views.Length > 0) {
            var index = (Array.IndexOf(views, Target) + 1) % views.Length;
            Target = views[index];
        }
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Equals))
            CycleTarget();
    }
    public void LateUpdate() {
        if (Target)
            transform.position = Target.body ? Target.body.transform.position : Target.transform.position;
    }
    public UnitView Target {
        get => target;
        set {
            target = value;
            if (!value)
                return;
            if (manualControl)
                manualControl.target = target.transform;
        }
    }
}