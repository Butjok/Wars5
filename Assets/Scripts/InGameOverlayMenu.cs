using UnityEngine;
using UnityEngine.Assertions;

public class InGameOverlayMenu : StateMachineState {

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

	public InGameOverlayMenu(StateMachine game) : base(game, nameof(InGameOverlayMenu)) {
		visible = new ChangeTracker<bool>(_ => View.Visible = visible.v);
		visible.v = true;
	}

	public override void Update() {
		base.Update();
		
		if (Input.GetKeyDown(KeyCode.Escape))
			Sm.Pop();
	}

	public override void Dispose() {
		base.Dispose();
		visible.v = false;
	}

	public ChangeTracker<bool> visible;
}