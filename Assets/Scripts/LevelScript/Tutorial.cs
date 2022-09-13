using UnityEngine;

public class Tutorial : LevelLogic {
	
	public Tutorial(Game2 game) : base(game) { }
	
	public override void OnTurnStart() {
		if (game.Turn == 0)
			game.state.PauseTo(new DialogueState(game, new[] {
				new DialogueUi.Speech {
					speaker = DialogueSpeaker.Natalie,
					lines = new[] {
						new DialogueUi.Line { text = "Hello!" },
						new DialogueUi.Line { text = "This is Wars3D!" },
					}
				},
				new DialogueUi.Speech {
					speaker = DialogueSpeaker.Vladan,
					lines = new[] {
						new DialogueUi.Line { text = "This is invasion!" },
					}
				},
			}));
	}

	public override void OnVictory() {
		Debug.Log("Well played!");
	}

	public override void OnDefeat() {
		Debug.Log("You lost! How could you...");
	}
}