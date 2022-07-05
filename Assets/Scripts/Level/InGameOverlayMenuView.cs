using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

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

public class InGameOverlayMenu : SubstateMachine {

	public static Lazy<InGameOverlayMenu> instance = new(() => new InGameOverlayMenu());
	private InGameOverlayMenuView _view;
	private InGameOverlayMenuView View {
		get {
			if (!_view) {
				_view = Object.FindObjectOfType<InGameOverlayMenuView>(true);
				Assert.IsTrue(_view);
			}
			return _view;
		}
	}

	public InGameOverlayMenu() : base(null, nameof(InGameOverlayMenu)) {
		visible = new ChangeTracker<bool>(_ => View.Visible = visible.v);
		visible.v = true;
	}

	public override void Dispose() {
		base.Dispose();
		visible.v = false;
	}

	public ChangeTracker<bool> visible;
}