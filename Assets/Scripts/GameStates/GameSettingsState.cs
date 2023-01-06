using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public const string prefix = "game-settings-state.";

    public const string close = prefix + "close";
    
    public static IEnumerator Run(Main main) {

        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);
        menu.Show(main);

        while (true) {
            yield return null;

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize())
                    switch (token) {
                        
                        case close:
                            menu.Hide();
                            yield break;
                        
                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}