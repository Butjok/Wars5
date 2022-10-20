using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameMenuState {
    public static IEnumerator New(Game game) {
        
        var oldInput = game.input;
        game.input = new InputCommandsContext();

        var menuView = Object.FindObjectOfType<GameMenu>(true);
        Assert.IsTrue(menuView);

        menuView.Show(game);

        while (true) {
            yield return null;

            if (game.input.cancel) {
                menuView.Hide();
                game.input = oldInput;
                yield break;
            }
        }
    }
}