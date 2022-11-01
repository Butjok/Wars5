using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public static bool shouldReturn;

    public static IEnumerator New(Game game) {

        shouldReturn = false;
        
        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);

        menu.Show(game);

        while (true) {
            yield return null;

            if (shouldReturn) {
                shouldReturn = false;
                
                menu.Hide();
                yield break;
            }
        }
    }
}