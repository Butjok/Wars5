using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HardFollow : MonoBehaviour {
	public UnitView[] views = { };
	public UnitView target;
	public bool selectRandomTarget = false;
	public ManualControl manualControl;
	public void Start() {
		if (selectRandomTarget) {
			views = FindObjectsOfType<UnitView>();
			if (views.Length > 0)
				Target = views[0];
		}
	}
	public void Update() {
		if (Input.GetKeyDown(KeyCode.Equals) && views.Length > 0) 
			Target = views[(Array.IndexOf(views, Target) + 1) % views.Length];
	}
	public void LateUpdate() {
		transform.position =Target.body ? Target.body.transform.position: Target.transform.position;
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