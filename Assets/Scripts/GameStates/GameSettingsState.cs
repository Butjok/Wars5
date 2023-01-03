using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public static bool shouldBreak;

    public static IEnumerator New(Main main) {

        shouldBreak = false;
        
        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);

        menu.Show(main);

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