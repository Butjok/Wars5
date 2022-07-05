using UnityEngine;
using UnityEngine.Assertions;

public class InGameOverlayMenuView : MonoBehaviour {

	public bool Visible {
		set => gameObject.SetActive(value);
	}
	
	public void Awake() {
		DontDestroyOnLoad(gameObject);
	}
}

public class InGameOverlayMenu :SubstateMachine {

	public static Lazy<InGameOverlayMenu> instance = new(()=>new InGameOverlayMenu());
	private InGameOverlayMenuView view;
	public InGameOverlayMenuView View {
		get {
			if (!view) {
				view = Object.FindObjectOfType<InGameOverlayMenuView>();
				Assert.IsTrue(view);
			}
			return view;
		}
	}
	
	public ChangeTracker<bool> visible;
}