using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameMenuState {

    public const string prefix = "game-menu-state.";

    public const string close = prefix + "close";
    public const string openSettingsMenu = prefix + "open-settings-menu";
    public const string openLoadGameMenu = prefix + "open-load-game-menu";

    public static IEnumerator Run(Main main) {

        var menu = Object.FindObjectOfType<GameMenu>(true);
        Assert.IsTrue(menu);

        PlayerView.globalVisibility = false;
        yield return null;

        CursorView.TryFind(out var cursor);
        if (cursor)
            cursor.Visible = false;
        CameraRig.Instance.enabled = false;

        menu.Show(main);

        while (true) {
            yield return null;

            while (main.commands.TryDequeue(out var input))
                foreach (var token in input.Tokenize())
                    switch (token) {

                        case close:
                            menu.Hide();
                            if (cursor)
                                cursor.Visible = true;
                            CameraRig.Instance.enabled = true;
                            PlayerView.globalVisibility = true;
                            yield break;

                        case openSettingsMenu:
                            menu.Hide();
                            yield return GameSettingsState.Run(main);
                            menu.Show(main);
                            break;

                        case openLoadGameMenu:
                            menu.Hide();
                            yield return LoadGameState.Run(main);
                            menu.Show(main);
                            break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}