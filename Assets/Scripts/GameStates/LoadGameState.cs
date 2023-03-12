using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class LoadGameState {

    public const string prefix = "load-game-state.";

    public const string close = prefix + "close";
    
    public static IEnumerator<StateChange> Run(Level level) {

        var saves = SaveEntry.FileNames
            .Select(SaveEntry.Read)
            .OrderByDescending(saveData => saveData.dateTime);

        var menu = Object.FindObjectOfType<LoadGameMenu>(true);
        Assert.IsTrue(menu);
        menu.Show(level, saves);

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