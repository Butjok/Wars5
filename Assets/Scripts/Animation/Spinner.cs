using System;
using DG.Tweening;
using UnityEngine;

public class Spinner : MonoBehaviour {

	public enum Axis { X, Y, Z }

	public float timeMultiplier = 1;
	public AnimationCurve curve = AnimationCurve.Linear(0, 0, 4, 360);
	public float stopDuration = .1f;
	public float startDuration = 3;
	public float time;
	public Tweener tweener;
	public Axis axis = Axis.Y;

	public void Update() {
		var angle = curve.Evaluate(time % curve[curve.length - 1].time);
		transform.localRotation = axis switch {
			Axis.X => Quaternion.Euler(angle, 0, 0),
			Axis.Y => Quaternion.Euler(0, angle, 0),
			Axis.Z => Quaternion.Euler(0, 0, angle),
			_ => throw new ArgumentOutOfRangeException()
		};
		time += timeMultiplier * Time.deltaTime;
	}
	[ContextMenu(nameof(StopSpinning))]
	public void StopSpinning() {
		tweener?.Kill();
		tweener = DOTween.To(value => timeMultiplier = value, timeMultiplier, 0, stopDuration);
	}
	[ContextMenu(nameof(StartSpinning))]
	public void StartSpinning() {
		tweener?.Kill();
		tweener = DOTween.To(value => timeMultiplier = value, timeMultiplier, 1, startDuration);
	}
}