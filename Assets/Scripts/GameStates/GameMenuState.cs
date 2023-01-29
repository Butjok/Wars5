using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameMenuState {

    public const string prefix = "game-menu-state.";

    public const string close = prefix + "close";
    public const string openSettingsMenu = prefix + "open-settings-menu";
    public const string openLoadGameMenu = prefix + "open-load-game-menu";

    public static IEnumerator<StateChange> Run(Main main) {

        var menu = Object.FindObjectOfType<GameMenu>(true);
        Assert.IsTrue(menu);

        PlayerView.globalVisibility = false;
        yield return StateChange.none;

        CursorView.TryFind(out var cursor);
        CameraRig.TryFind(out var cameraRig);
        
        if (cursor)
            cursor.show = false;
        if (cameraRig)
            cameraRig.enabled = false;

        menu.Show(main);

        while (true) {
            yield return StateChange.none;

            while (main.commands.TryDequeue(out var input))
                foreach (var token in Tokenizer.Tokenize(input))
                    switch (token) {

                        case close:
                            menu.Hide();
                            if (cursor)
                                cursor.show = true;
                            if (cameraRig)
                                cameraRig.enabled = true;
                            PlayerView.globalVisibility = true;
                            yield return StateChange.Pop();
                            break;

                        case openSettingsMenu:
                            menu.Hide();
                            yield return StateChange.Push("settings", GameSettingsState.Run(main));
                            menu.Show(main);
                            break;

                        case openLoadGameMenu:
                            menu.Hide();
                            yield return StateChange.Push("load-game",LoadGameState.Run(main));
                            menu.Show(main);
                            break;

                        default:
                            main.stack.ExecuteToken(token);
                            break;
                    }
        }
    }
}