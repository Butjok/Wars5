using UnityEngine;

public class DialogueState : State2<Game2> {

	public static DialogueUi ui;
	public DialogueUi.Speech[] speeches;

	public DialogueState(Game2 parent, DialogueUi.Speech[] speeches) : base(parent) {
		this.speeches = speeches;
	}

	public override void Start() {
		
		if (!ui) {
			var prefab = Resources.Load<DialogueUi>(nameof(DialogueUi));
			ui = Object.Instantiate(prefab);
			Object.DontDestroyOnLoad(ui.gameObject);
			ui.Visible = false;
		}
		
		ui.speeches = speeches;
		ui.index = Vector2Int.zero;
		ui.onEnd = UnpauseLastState;
		ui.Visible = true;
	}
}