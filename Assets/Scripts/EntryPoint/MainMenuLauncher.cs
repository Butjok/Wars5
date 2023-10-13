using UnityEngine;

public class MainMenuLauncher : MonoBehaviour {

    public bool showSplashScreen = true;
    public bool showWelcomeScreen = true;
    
    private void Start() {

        var game = Game.Instance;
        if (game.stateMachine.Count != 0)
            return;

        game.stateMachine.Push(new GameSessionState(game));
        game.EnqueueCommand(GameSessionState.Command.LaunchMainMenu, (showSplashScreen, showWelcomeScreen));
    }
}