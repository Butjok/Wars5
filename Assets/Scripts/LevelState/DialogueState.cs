using UnityEngine;

public class DialogueState : State2<Game2> {

	public static DialogueUi ui;

	public DialogueState(Game2 parent, DialogueUi.Speech[] speeches) : base(parent) {
		if (!ui) {
			var prefab = Resources.Load<DialogueUi>(nameof(DialogueUi));
			ui = Object.Instantiate(prefab);
			Object.DontDestroyOnLoad(ui.gameObject);
			ui.gameObject.SetActive(false);
		}
		ui.speeches = speeches;
		ui.gameObject.SetActive(true);
		ui.onEnd += UnpauseLastState;
	}
	public override void Dispose() {
		ui.onEnd -= UnpauseLastState;
	}
}