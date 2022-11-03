using System.Collections;
using System.Linq;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

public static class LoadGameState {

    public static bool shouldBreak;

    public static IEnumerator New(Game game) {

        shouldBreak = false;

        var saves = SaveEntry.FileNames
            .Select(SaveEntry.Read)
            .OrderByDescending(saveData => saveData.dateTime);

        var menu = Object.FindObjectOfType<LoadGameMenu>(true);
        Assert.IsTrue(menu);

        menu.Show(game, saves);

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