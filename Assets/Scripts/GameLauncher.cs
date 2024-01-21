using UnityEngine;

public class GameLauncher : MonoBehaviour {

    public bool showSplashScreen;
    public bool showWelcome;
    public string loadOnAwakeFileName;

    private void Start() {
        var game = Game.Instance;
        if (game.stateMachine.Count != 0)
            return;

        game.stateMachine.Push(new GameSessionState(game));

        string input = null;
        if (!string.IsNullOrWhiteSpace(loadOnAwakeFileName))
            input = LevelEditorFileSystem.TryReadLatest(loadOnAwakeFileName);

        if (input != null) {
            game.EnqueueCommand(GameSessionState.Command.OpenLevelEditor, (input, false));
        }
        else {
            game.EnqueueCommand(GameSessionState.Command.LaunchMainMenu, (showSplashScreen, showWelcome));
        }
    }
}