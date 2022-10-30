using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameSettingsState {

    public static bool shouldExit;

    public static IEnumerator New(Game game) {

        shouldExit = false;
        
        var menu = Object.FindObjectOfType<GameSettingsMenu>(true);
        Assert.IsTrue(menu);
        
        menu.Show(game);
        CursorView.Instance.Visible = false;
        CameraRig.Instance.enabled = false;
        
        while (true) {
            yield return null;

            if (shouldExit) {
                menu.Hide();
                CursorView.Instance.Visible = true;
                CameraRig.Instance.enabled = true;
                yield break;
            }
        }
    }
}