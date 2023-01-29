using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public const string prefix = "game-settings-state.";

    public const string close = prefix + "close";
    
    public static IEnumerator<StateChange> Run(Main main) {

        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);
        menu.Show(main);

        while (true) {
            yield return StateChange.none;

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {
                        
                        case close:
                            menu.Hide();
                            yield return StateChange.Pop();
                            break;
                        
                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}