using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public const string prefix = "game-settings-state.";

    public const string close = prefix + "close";
    
    public static IEnumerator<StateChange> Run(Level level) {

        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);
        menu.Show(level);

        while (true) {
            yield return StateChange.none;

            while (level.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {
                        
                        case close:
                            menu.Hide();
                            yield return StateChange.Pop();
                            break;
                        
                        default:
                            level.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}