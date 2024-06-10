using UnityEngine;

public class GameLauncher : MonoBehaviour {

    public bool showSplashScreen;
    public bool showWelcome;
    public bool loadMainMenu;
    public bool loadOnAwake;
    public string loadOnAwakeFileName;

    private void Start() {
        var game = Game.Instance;
        if (game.stateMachine.Count != 0)
            return;

        game.stateMachine.Push(new GameSessionState(game));

        if (loadMainMenu) {
            game.EnqueueCommand(GameSessionState.Command.LaunchMainMenu, (showSplashScreen, showWelcome));
            return;
        }

        string input = null;
        if (loadOnAwake)
            input = LevelEditorFileSystem.TryReadLatest(loadOnAwakeFileName);

        game.EnqueueCommand(GameSessionState.Command.OpenLevelEditor, (input, false));
    }
}