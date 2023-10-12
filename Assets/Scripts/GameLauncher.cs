using UnityEngine;
using UnityEngine.Assertions;

public class GameLauncher : MonoBehaviour {

    public bool showSplashScreen;
    public bool showWelcome;
    public TextAsset save;
    public string saveName;
    public bool startInLevelEditor;
    public bool showLevelEditorTileMesh = false;

    private void Start() {

        var game = Game.Instance;
        if (game.stateMachine.Count != 0)
            return;

        game.stateMachine.Push(new GameSessionState(game));
        
        var input = save ? save.text
            : !string.IsNullOrWhiteSpace(saveName) ? LevelEditorFileSystem.TryReadLatest(saveName)
            : null;

        if (input != null)
            if (startInLevelEditor)
                game.EnqueueCommand(GameSessionState.Command.OpenLevelEditor, (input, showLevelEditorTileMesh));
            else
                game.EnqueueCommand(GameSessionState.Command.PlayLevel, input);
        else
            game.EnqueueCommand(GameSessionState.Command.LaunchEntryPoint, (showSplashScreen, showWelcome));
    }
}