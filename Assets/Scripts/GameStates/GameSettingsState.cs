using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public static bool shouldBreak;

    public static IEnumerator New(Game game) {

        shouldBreak = false;
        
        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);

        menu.Show(game);

        while (true) {
            yield return null;

            if (shouldBreak) {
                shouldBreak = false;
                
                menu.Hide();
                yield break;
            }
        }
    }
}