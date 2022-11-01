using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameMenuState {

    public static bool shouldResume;
    public static bool shouldOpenSettings;
    public static bool shouldLoadGame;

    public static IEnumerator New(Game game) {

        shouldResume = false;
        shouldOpenSettings = false;
        shouldLoadGame = false;

        var menu = Object.FindObjectOfType<GameMenu>(true);
        Assert.IsTrue(menu);

        PlayerView.globalVisibility = false;
        yield return null;
        CursorView.Instance.Visible = false;
        CameraRig.Instance.enabled = false;

        menu.Show(game);
        
        while (true) {
            yield return null;

            if (shouldResume) {
                shouldResume = false;
                
                menu.Hide();
                CursorView.Instance.Visible = true;
                CameraRig.Instance.enabled = true;
                PlayerView.globalVisibility = true;
                yield break;
            }
            
            if (shouldOpenSettings) {
                shouldOpenSettings = false;
                
                menu.Hide();
                yield return GameSettingsState.New(game);
                menu.Show(game);
            }

            if (shouldLoadGame) {
                shouldLoadGame = false;
                
                menu.Hide();
                yield return LoadGameState.New(game);
                menu.Show(game);
            }
        }
    }
}