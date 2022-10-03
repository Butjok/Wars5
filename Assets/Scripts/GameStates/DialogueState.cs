using System.Collections;
using UnityEngine;

public static class DialogueState {

	public static IEnumerator New(Game game, DialogueUi.Speech[] speeches) {

		var ui = DialogueUi.Instance;
		var index = Vector2Int.zero;
		
		ui.Visible = true;
		ui.Refresh(speeches[0].speaker, speeches[0].lines[0]);

		while (true) {
			yield return null;

			if (Input.GetKeyDown(KeyCode.Space)) {

				var speech = speeches[index[0]];

				if (index[1] < speech.lines.Length - 1)
					index[1]++;
				else {
					index[0]++;
					index[1] = 0;
				}

				if (index[0] >= 0 && index[0] < speeches.Length &&
				    index[1] >= 0 && index[1] < speeches[index[0]].lines.Length) {

					speech = speeches[index[0]];
					var line = speech.lines[index[1]];
					ui.Refresh(speech.speaker, line);
				}
				else {
					ui.Visible = false;
					yield break;
				}
			}
		}
	}
}