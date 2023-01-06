using System.Collections;
using UnityEngine;

public static class DialogueState {

    public const string prefix = "dialogue-state.";
    public const string next = prefix + "next";
    
    public static IEnumerator Run(Main main, DialogueUi.Speech[] speeches) {

        var ui = DialogueUi.Instance;
        var index = Vector2Int.zero;

        ui.Visible = true;
        ui.Set(speeches[0].speaker, speeches[0].lines[0]);

        while (true) {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Space))
                main.commands.Enqueue(next);

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize())
                    switch (token) {

                        case next: {
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
                                ui.Set(speech.speaker, line);
                            }
                            else {
                                ui.Visible = false;
                                yield break;
                            }
                            
                            break;
                        }

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}