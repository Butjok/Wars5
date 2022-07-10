using DG.Tweening;
using UnityEngine;

public class InGameOverlayMenuView : MonoBehaviour {

	public float timeScaleFadeDuration = 1;
	public bool Visible {
		set {
			if (value)
				DOTween.To(value => Time.timeScale = value, 1, 0, timeScaleFadeDuration).SetUpdate(true);
			else
				DOTween.To(value => Time.timeScale = value, 0, 1, timeScaleFadeDuration).SetUpdate(true);
		}
	}

	public void Awake() {
		DontDestroyOnLoad(gameObject);
	}
}