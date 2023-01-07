using System.Collections;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class LoadGameState {

    public const string prefix = "load-game-state.";

    public const string close = prefix + "close";
    
    public static IEnumerator Run(Main main) {

        var saves = SaveEntry.FileNames
            .Select(SaveEntry.Read)
            .OrderByDescending(saveData => saveData.dateTime);

        var menu = Object.FindObjectOfType<LoadGameMenu>(true);
        Assert.IsTrue(menu);
        menu.Show(main, saves);

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