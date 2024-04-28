using System.IO;
using System.Text;
using Stable;
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
        if (input == null) {
            var stringWriter =  new StringWriter();
            stringWriter.PostfixWrite(@"

// Blue
player {{
    :color-name          ( ColorName Blue enum )
    :team                ( Team None enum )
    :co-name             ( PersonName Natalie enum )
    :ui-position         ( 0 1 int2 )
    :credits             ( 0 )
    :power-meter         ( 0 )
    :unit-look-direction ( 1 0 int2 )
    :side                ( Side Left enum )
    .mark-as-local
    .add
}}
// Red
player {{
    :color-name          ( ColorName Red enum )
    :team                ( Team None enum )
    :co-name             ( PersonName Vladan enum )
    :ui-position         ( 1 1 int2 )
    :credits             ( 0 )
    :power-meter         ( 0 )
    :unit-look-direction ( -1 0 int2 )
    :side                ( Side Right enum )
    .add
}}");
            input = stringWriter.ToString();
            Debug.Log(input);
        }

        game.EnqueueCommand(GameSessionState.Command.OpenLevelEditor, (input, false));
    }
}